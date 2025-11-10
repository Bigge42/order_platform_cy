using System;

namespace HDPro.CY.Order.Models
{
    /// <summary>
    /// ERP：销售订单备注（SalEntryBZToDDPT）DTO
    /// </summary>
    public class ErpEntryDto
    {
        public long FENTRYID { get; set; }
        public string F_BLN_BZ1 { get; set; } // 备注 -> remark_raw
        public string F_BLN_BZ { get; set; } // 内部信息传递 -> internal_note
        public string FSPECIFICATION { get; set; }
        public string FMTONO { get; set; }
        public string F_BLN_CONTACTNONAME { get; set; } // 合同号
        public string F_BLN_CPXH { get; set; } // 产品型号
        public string F_BLN_FGUID { get; set; } // 选型ID
    }

    /// <summary>
    /// ERP：销售变更备注（SalChangeBZToDDPT）DTO
    /// </summary>
    public class ErpChangeDto
    {
        public long FENTRYID { get; set; }
        public string F_BLN_BZ1 { get; set; } // 备注 -> remark_raw
        public string F_BLN_BZ { get; set; } // 内部信息传递 -> internal_note
        public string FMTONO { get; set; }
    }
}
