    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Threading.Tasks;
    using Microsoft.Data.SqlClient; // 若环境未装可改为 System.Data.SqlClient
    using HDPro.Core.EFDbContext;
    using HDPro.Core.Extensions.AutofacManager;
    using HDPro.CY.Order.IRepositories;
    using HDPro.CY.Order.Models.MaterialCallBoardDtos;
    using Microsoft.EntityFrameworkCore;

    namespace HDPro.CY.Order.Repositories
    {
        // 提示：类头、基类与接口由“生成的”partial声明，这里只放方法与工具函数
        public partial class MaterialCallBoardRepository : IMaterialCallBoardRepository
        {
            // 通过容器取 EF 上下文（不新增构造函数，避免与生成构造冲突）
            private ServiceDbContext Ctx => AutofacContainerModule.GetService<ServiceDbContext>();

            private const string TargetSchema = "dbo";
            private const string TargetTable = "MaterialCallBoard";
            private string TargetFullName => $"[{TargetSchema}].[{TargetTable}]";

            /// <summary>
            /// 批量Upsert到 dbo.MaterialCallBoard（WorkOrderNo 作为匹配键）
            /// - 批内去重（后者覆盖前者）
            /// - SqlBulkCopy -> 临时表 -> MERGE
            /// - 在 ON 条件处把 S.WorkOrderNo COLLATE 成目标列排序规则，消除跨库/跨列Collation冲突
            /// </summary>
            public async Task<(int inserted, int updated)> BulkUpsertAsync(List<MaterialCallBoardBatchDto> rows)
            {
                if (rows == null || rows.Count == 0) return (0, 0);

                // 批内去重
                var dict = new Dictionary<string, MaterialCallBoardBatchDto>(StringComparer.OrdinalIgnoreCase);
                foreach (var r in rows)
                {
                    if (r == null || string.IsNullOrWhiteSpace(r.WorkOrderNo)) continue;
                    dict[r.WorkOrderNo] = r;
                }

                var dt = BuildDataTable(dict.Values);

                // 拿连接串（优先 EF Core 提供的连接串）
                var connStr = Ctx.Database.GetConnectionString();
                if (string.IsNullOrWhiteSpace(connStr))
                    connStr = Ctx.Database.GetDbConnection().ConnectionString;

                using var conn = new SqlConnection(connStr);
                await conn.OpenAsync();
                using var tran = conn.BeginTransaction();

                try
                {
                    // 读取目标列排序规则
                    var workOrderCollation = await GetColumnCollationAsync(
                        conn, tran, TargetSchema, TargetTable, "WorkOrderNo"
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
                        await cmd.ExecuteNonQueryAsync();

                    // 2) 批量写入
                    using (var bulk = new SqlBulkCopy(conn, SqlBulkCopyOptions.CheckConstraints, tran))
                    {
                        bulk.DestinationTableName = "#MaterialCallBoard_Import";
                        bulk.BulkCopyTimeout = 0;
                        bulk.ColumnMappings.Add("WorkOrderNo", "WorkOrderNo");
                        bulk.ColumnMappings.Add("PlanTrackNo", "PlanTrackNo");
                        bulk.ColumnMappings.Add("ProductCode", "ProductCode");
                        bulk.ColumnMappings.Add("CallerName", "CallerName");
                        bulk.ColumnMappings.Add("CalledAt", "CalledAt");
                        await bulk.WriteToServerAsync(dt);
                    }

                    // 3) MERGE
                    var target = TargetFullName;
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
      INSERT(WorkOrderNo, PlanTrackNo, ProductCode, CallerName, CalledAt)
      VALUES(S.WorkOrderNo, S.PlanTrackNo, S.ProductCode, S.CallerName, S.CalledAt)
    OUTPUT $action INTO #out;

    SELECT
      SUM(CASE WHEN action='INSERT' THEN 1 ELSE 0 END) AS Inserted,
      SUM(CASE WHEN action='UPDATE' THEN 1 ELSE 0 END) AS Updated
    FROM #out;";

                    int inserted = 0, updated = 0;
                    using (var cmd = new SqlCommand(mergeSql, conn, tran))
                    using (var rd = await cmd.ExecuteReaderAsync())
                    {
                        if (await rd.ReadAsync())
                        {
                            inserted = rd.IsDBNull(0) ? 0 : rd.GetInt32(0);
                            updated = rd.IsDBNull(1) ? 0 : rd.GetInt32(1);
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

            private static DataTable BuildDataTable(IEnumerable<MaterialCallBoardBatchDto> list)
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

            private static async Task<string?> GetColumnCollationAsync(
                SqlConnection conn, SqlTransaction tran, string schema, string table, string column)
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
