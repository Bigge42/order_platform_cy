using System;
using System.Collections.Generic;
using System.Text;

namespace HDPro.Core.DBManager
{
    public class DBConnectionAttribute : Attribute
    {
        public string DBName { get; set; }
    }
}
