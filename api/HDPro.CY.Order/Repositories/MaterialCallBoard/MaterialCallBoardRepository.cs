using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using HDPro.CY.Order.IRepositories;
using HDPro.CY.Order.Models.MaterialCallBoardDtos;
using Microsoft.Data.SqlClient;                 // 如果缺包，可改为 System.Data.SqlClient
using Microsoft.Extensions.Configuration;

namespace HDPro.CY.Order.Repositories
{
    /// <summary>
    /// 直连 SQL Server 的仓储实现（SqlBulkCopy + MERGE）
    /// 默认连接 Connection:ServiceDbContext（OCP_Service）
    /// 解决跨排序规则冲突：在 MERGE 的 ON 条件中将 S.WorkOrderNo COLLATE 成目标列的排序规则
    /// </summary>
    public class MaterialCallBoardRepository : IMaterialCallBoardRepository
    {
        private readonly string _connStr;
        private readonly string _targetSchema = "dbo";
        private readonly string _targetTable = "MaterialCallBoard";

        public MaterialCallBoardRepository(IConfiguration config)
        {
            // ✅ 优先用业务库（OCP_Service）
            _connStr =
                config["Connection:ServiceDbContext"]              // OCP_Service
                ?? config["Connection:DbConnectionString"]         // OCP_Main
                ?? config.GetConnectionString("CYOrderDb")
                ?? config.GetConnectionString("DefaultConnection")
                ?? config["DbConnectionString"];

            if (string.IsNullOrWhiteSpace(_connStr))
                throw new InvalidOperationException(
                    "未找到 SQL Server 连接串。请配置 Connection:ServiceDbContext 或 Connection:DbConnectionString");
        }

        private string BuildTargetName() => $"[{_targetSchema}].[{_targetTable}]";

        public async Task<(int inserted, int updated)> BulkUpsertAsync(List<MaterialCallBoardBatchDto> rows)
        {
            if (rows == null || rows.Count == 0) return (0, 0);

            // 批内去重（WorkOrderNo 后者覆盖前者）
            var latest = new Dictionary<string, MaterialCallBoardBatchDto>(StringComparer.OrdinalIgnoreCase);
            foreach (var r in rows)
            {
                if (r == null || string.IsNullOrWhiteSpace(r.WorkOrderNo)) continue;
                latest[r.WorkOrderNo] = r;
            }

            var table = BuildDataTable(latest.Values.ToList());
            var target = BuildTargetName(); // [dbo].[MaterialCallBoard]

            using var conn = new SqlConnection(_connStr);
            await conn.OpenAsync();
            using var tran = conn.BeginTransaction();

            try
            {
                // 读取目标表关键列（WorkOrderNo）的排序规则，用于消除 MERGE 的 ON 冲突
                var workOrderCollation = await GetColumnCollationAsync(
                    conn, tran, _targetSchema, _targetTable, "WorkOrderNo"
                ) ?? "DATABASE_DEFAULT";

                // 1) 临时表
                const string createTemp = @"
CREATE TABLE #MaterialCallBoard_Import
(
    WorkOrderNo NVARCHAR(100) NOT NULL,
    PlanTrackNo NVARCHAR(100) NOT NULL,
    ProductCode NVARCHAR(100) NOT NULL,
    CallerName  NVARCHAR(100) NOT NULL,
    CalledAt    DATETIME2     NOT NULL
);";
                using (var cmd = new SqlCommand(createTemp, conn, tran))
                {
                    await cmd.ExecuteNonQueryAsync();
                }

                // 2) 批量写入临时表
                using (var bulk = new SqlBulkCopy(conn, SqlBulkCopyOptions.CheckConstraints, tran))
                {
                    bulk.DestinationTableName = "#MaterialCallBoard_Import";
                    bulk.BulkCopyTimeout = 0;
                    bulk.ColumnMappings.Add("WorkOrderNo", "WorkOrderNo");
                    bulk.ColumnMappings.Add("PlanTrackNo", "PlanTrackNo");
                    bulk.ColumnMappings.Add("ProductCode", "ProductCode");
                    bulk.ColumnMappings.Add("CallerName", "CallerName");
                    bulk.ColumnMappings.Add("CalledAt", "CalledAt");
                    await bulk.WriteToServerAsync(table);
                }

                // 3) MERGE 到正式表
                // 关键：在 ON 条件处将 S.WorkOrderNo COLLATE 成目标列的排序规则，避免 Chinese_PRC_CI_AS 与 SQL_Latin1_General_CP1_CI_AS 冲突
                var mergeSql = $@"
CREATE TABLE #out(action NVARCHAR(10));

MERGE {target} AS T
USING #MaterialCallBoard_Import AS S
    ON T.WorkOrderNo = S.WorkOrderNo COLLATE {workOrderCollation}
WHEN MATCHED THEN
    UPDATE SET
        T.PlanTrackNo = S.PlanTrackNo,
        T.ProductCode = S.ProductCode,
        T.CallerName  = S.CallerName,
        T.CalledAt    = S.CalledAt
WHEN NOT MATCHED BY TARGET THEN
    INSERT (WorkOrderNo, PlanTrackNo, ProductCode, CallerName, CalledAt)
    VALUES (S.WorkOrderNo, S.PlanTrackNo, S.ProductCode, S.CallerName, S.CalledAt)
OUTPUT $action INTO #out;

SELECT
    SUM(CASE WHEN action='INSERT' THEN 1 ELSE 0 END) AS Inserted,
    SUM(CASE WHEN action='UPDATE' THEN 1 ELSE 0 END) AS Updated
FROM #out;";
                int inserted = 0, updated = 0;
                using (var cmd = new SqlCommand(mergeSql, conn, tran))
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        inserted = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
                        updated = reader.IsDBNull(1) ? 0 : reader.GetInt32(1);
                    }
                }

                tran.Commit();
                return (inserted, updated);
            }
            catch
            {
                try { tran.Rollback(); } catch { /* ignore */ }
                throw;
            }
        }

        /// <summary>
        /// 读取指定列的排序规则（collation）。若列为非字符类型或未设置，返回 null。
        /// </summary>
        private static async Task<string?> GetColumnCollationAsync(
            SqlConnection conn, SqlTransaction tran, string schema, string table, string column)
        {
            const string sql = @"
SELECT c.collation_name
FROM sys.columns c
JOIN sys.tables  t ON t.object_id = c.object_id
JOIN sys.schemas s ON s.schema_id = t.schema_id
WHERE s.name = @schema AND t.name = @table AND c.name = @column;";
            using var cmd = new SqlCommand(sql, conn, tran);
            cmd.Parameters.AddWithValue("@schema", schema);
            cmd.Parameters.AddWithValue("@table", table);
            cmd.Parameters.AddWithValue("@column", column);
            var obj = await cmd.ExecuteScalarAsync();
            if (obj == null || obj is DBNull) return null;
            var name = (string)obj;
            // 一般返回如：SQL_Latin1_General_CP1_CI_AS / Chinese_PRC_CI_AS 等
            return string.IsNullOrWhiteSpace(name) ? null : name;
        }

        private static DataTable BuildDataTable(List<MaterialCallBoardBatchDto> list)
        {
            var dt = new DataTable();
            dt.Columns.Add("WorkOrderNo", typeof(string));
            dt.Columns.Add("PlanTrackNo", typeof(string));
            dt.Columns.Add("ProductCode", typeof(string));
            dt.Columns.Add("CallerName", typeof(string));
            dt.Columns.Add("CalledAt", typeof(DateTime));

            var now = DateTime.Now;
            foreach (var d in list)
            {
                dt.Rows.Add(
                    d.WorkOrderNo?.Trim(),
                    d.PlanTrackNo?.Trim(),
                    d.ProductCode?.Trim(),
                    d.CallerName?.Trim(),
                    d.CalledAt ?? now
                );
            }
            return dt;
        }
    }
}
