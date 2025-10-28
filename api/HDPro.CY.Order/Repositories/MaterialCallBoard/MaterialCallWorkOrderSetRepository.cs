using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using HDPro.CY.Order.IRepositories.MaterialCallBoard;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace HDPro.CY.Order.Repositories.MaterialCallBoard
{
    public class MaterialCallWorkOrderSetRepository : IMaterialCallWorkOrderSetRepository
    {
        private readonly string _connStr;
        private const string SetSchema = "dbo";
        private const string SetTable = "MaterialCallWorkOrderSet";
        private const string McbSchema = "dbo";
        private const string McbTable = "MaterialCallBoard";

        public MaterialCallWorkOrderSetRepository(IConfiguration cfg)
        {
            _connStr =
                cfg["Connection:ServiceDbContext"] ??
                cfg["Connection:DbConnectionString"] ??
                cfg.GetConnectionString("CYOrderDb") ??
                cfg.GetConnectionString("DefaultConnection") ??
                cfg["DbConnectionString"] ??
                throw new InvalidOperationException("未找到 SQL Server 连接串（ServiceDbContext/DbConnectionString）。");
        }

        public async Task<(int total, int matched)> RefreshSnapshotAsync(List<string> codes)
        {
            // 准备导入数据
            var dt = new DataTable();
            dt.Columns.Add("WorkOrderCode", typeof(string));
            foreach (var c in codes)
            {
                if (!string.IsNullOrWhiteSpace(c))
                    dt.Rows.Add(c.Trim());
            }

            using var conn = new SqlConnection(_connStr);
            await conn.OpenAsync();
            using var tran = conn.BeginTransaction();

            try
            {
                // 取叫料看板 WorkOrderNo 列的排序规则，避免跨库/跨列 Collation 冲突
                var workOrderCollation = await GetColumnCollationAsync(conn, tran, McbSchema, McbTable, "WorkOrderNo")
                                          ?? "DATABASE_DEFAULT";

                // 临时表
                const string createTmp = @"
CREATE TABLE #MCW_Set_Import
(
    WorkOrderCode NVARCHAR(100) NOT NULL
);";
                using (var cmd = new SqlCommand(createTmp, conn, tran))
                    await cmd.ExecuteNonQueryAsync();

                // 批量写入临时表
                using (var bulk = new SqlBulkCopy(conn, SqlBulkCopyOptions.CheckConstraints, tran))
                {
                    bulk.DestinationTableName = "#MCW_Set_Import";
                    bulk.BulkCopyTimeout = 0;
                    bulk.ColumnMappings.Add("WorkOrderCode", "WorkOrderCode");
                    await bulk.WriteToServerAsync(dt);
                }

                // 全量替换集合：TRUNCATE + INSERT
                var setFullName = $"[{SetSchema}].[{SetTable}]";
                var mcbFullName = $"[{McbSchema}].[{McbTable}]";

                using (var cmd = new SqlCommand($@"
TRUNCATE TABLE {setFullName};
INSERT INTO {setFullName}(WorkOrderCode, ExistsInMCB)
SELECT WorkOrderCode, 0 FROM #MCW_Set_Import;

-- 计算 ExistsInMCB：在叫料看板存在则置 1
UPDATE S
SET S.ExistsInMCB = 1
FROM {setFullName} AS S
WHERE EXISTS(
    SELECT 1 FROM {mcbFullName} AS T
    WHERE T.WorkOrderNo = S.WorkOrderCode COLLATE {workOrderCollation}
);

-- 统计
SELECT 
    (SELECT COUNT(1) FROM {setFullName}) AS Total,
    (SELECT COUNT(1) FROM {setFullName} WHERE ExistsInMCB=1) AS Matched;
", conn, tran))
                using (var rd = await cmd.ExecuteReaderAsync())
                {
                    int total = 0, matched = 0;
                    if (await rd.ReadAsync())
                    {
                        total = rd.IsDBNull(0) ? 0 : rd.GetInt32(0);
                        matched = rd.IsDBNull(1) ? 0 : rd.GetInt32(1);
                    }
                    rd.Close();
                    tran.Commit();
                    return (total, matched);
                }
            }
            catch
            {
                try { tran.Rollback(); } catch { /* ignore */ }
                throw;
            }
        }

        public async Task<int> PruneMaterialCallBoardAsync()
        {
            using var conn = new SqlConnection(_connStr);
            await conn.OpenAsync();
            using var tran = conn.BeginTransaction();

            try
            {
                var workOrderCollation = await GetColumnCollationAsync(conn, tran, McbSchema, McbTable, "WorkOrderNo")
                                          ?? "DATABASE_DEFAULT";

                var setFullName = $"[{SetSchema}].[{SetTable}]";
                var mcbFullName = $"[{McbSchema}].[{McbTable}]";

                // 删除叫料看板中“不在白名单集合”的记录
                var sql = $@"
DELETE T
FROM {mcbFullName} AS T
LEFT JOIN {setFullName} AS S
  ON T.WorkOrderNo = S.WorkOrderCode COLLATE {workOrderCollation}
WHERE S.WorkOrderCode IS NULL;  -- 白名单不存在 → 删除
SELECT @@ROWCOUNT;";
                using (var cmd = new SqlCommand(sql, conn, tran))
                {
                    var affected = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                    tran.Commit();
                    return affected;
                }
            }
            catch
            {
                try { tran.Rollback(); } catch { /* ignore */ }
                throw;
            }
        }

        private static async Task<string?> GetColumnCollationAsync(SqlConnection conn, SqlTransaction tran, string schema, string table, string column)
        {
            const string sql = @"
SELECT c.collation_name
FROM sys.columns c
JOIN sys.tables  t ON t.object_id = c.object_id
JOIN sys.schemas s ON s.schema_id = t.schema_id
WHERE s.name=@schema AND t.name=@table AND c.name=@column;";
            using var cmd = new SqlCommand(sql, conn, tran);
            cmd.Parameters.AddWithValue("@schema", schema);
            cmd.Parameters.AddWithValue("@table", table);
            cmd.Parameters.AddWithValue("@column", column);
            var obj = await cmd.ExecuteScalarAsync();
            if (obj == null || obj is DBNull) return null;
            var name = (string)obj;
            return string.IsNullOrWhiteSpace(name) ? null : name;
        }
    }
}
