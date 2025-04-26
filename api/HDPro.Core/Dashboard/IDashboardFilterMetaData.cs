using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HDPro.Core.Dashboard
{
    public interface IDashboardFilterMetaData
    {
        (string sql, DynamicParameters parameters) OnActionExecuting(string sql, DynamicParameters parameters, string dbService, bool isProc, DateTime? date1, DateTime? date2, string filterType);
    }
}
