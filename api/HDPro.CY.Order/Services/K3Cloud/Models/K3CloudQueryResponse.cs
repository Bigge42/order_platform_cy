/*
 * K3Cloud查询响应模型
 */
using System.Collections.Generic;
using System.Dynamic;

namespace HDPro.CY.Order.Services.K3Cloud.Models
{
    /// <summary>
    /// K3Cloud查询响应模型
    /// </summary>
    /// <typeparam name="T">数据类型</typeparam>
    public class K3CloudQueryResponse<T>
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// 消息
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 错误堆栈跟踪
        /// </summary>
        public string ErrorStackTrace { get; set; }

        /// <summary>
        /// 数据
        /// </summary>
        public List<T> Data { get; set; } = new List<T>();
    }

    /// <summary>
    /// K3Cloud物料数据模型
    /// </summary>
    public class K3CloudMaterialData
    {
        /// <summary>
        /// 物料编号 - FNumber
        /// </summary>
        public string Number { get; set; }

        /// <summary>
        /// 物料名称 - FName
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 物料ID - FMaterialID
        /// </summary>
        public long? FMaterialID { get; set; }

        /// <summary>
        /// 物料属性 - FErpClsID
        /// </summary>
        public string FErpClsID { get; set; }

        /// <summary>
        /// 规格型号 - FSpecification
        /// </summary>
        public string FSpecification { get; set; }

        /// <summary>
        /// 产品型号 - F_BLN_CPXH
        /// </summary>
        public string F_BLN_CPXH { get; set; }

        /// <summary>
        /// 公称通径 - F_BLN_Gctj
        /// </summary>
        public string F_BLN_Gctj { get; set; }

        /// <summary>
        /// 公称压力 - F_BLN_Gcyl
        /// </summary>
        public string F_BLN_Gcyl { get; set; }

        /// <summary>
        /// CV值 - F_BLN_CV
        /// </summary>
        public string F_BLN_CV { get; set; }

        /// <summary>
        /// 附件 - F_BLN_Fj
        /// </summary>
        public string F_BLN_Fj { get; set; }

        /// <summary>
        /// 图号 - F_BLN_DwgNum
        /// </summary>
        public string F_BLN_DwgNum { get; set; }

        /// <summary>
        /// 材质 - F_BLN_Material
        /// </summary>
        public string F_BLN_Material { get; set; }

        /// <summary>
        /// 流量特性 - F_BLN_LLTX
        /// </summary>
        public string F_BLN_LLTX { get; set; }

        /// <summary>
        /// 填料形式 - F_BLN_TLXS
        /// </summary>
        public string F_BLN_TLXS { get; set; }

        /// <summary>
        /// 法兰连接方式 - F_BLN_FLLJFS
        /// </summary>
        public string F_BLN_FLLJFS { get; set; }

        /// <summary>
        /// 执行机构型号 - F_BLN_ZXJGXH
        /// </summary>
        public string F_BLN_ZXJGXH { get; set; }

        /// <summary>
        /// 执行机构行程 - F_BLN_ZXJGXC
        /// </summary>
        public string F_BLN_ZXJGXC { get; set; }

        /// <summary>
        /// 法兰标准 - F_BLN_Flbz
        /// </summary>
        public string F_BLN_Flbz { get; set; }

        /// <summary>
        /// 阀体材质 - F_BLN_Ftcz
        /// </summary>
        public string F_BLN_Ftcz { get; set; }

        /// <summary>
        /// 阀内件材质 - F_BLN_Fljcz
        /// </summary>
        public string F_BLN_Fljcz { get; set; }

        /// <summary>
        /// 法兰密封面型式 - F_BLN_Flmfmxs
        /// </summary>
        public string F_BLN_Flmfmxs { get; set; }

        /// <summary>
        /// TC发布人 - F_TC_RELEASER
        /// </summary>
        public string F_TC_RELEASER { get; set; }

        /// <summary>
        /// 库房编号 - FStockId.FNumber
        /// </summary>
        public string FStockNumber { get; set; }

        /// <summary>
        /// 车间ID - FWorkShopId
        /// </summary>
        public string FWorkShopId { get; set; }

        /// <summary>
        /// 是否有BOM - FIsBOM
        /// </summary>
        public string FIsBOM { get; set; }

        /// <summary>
        /// 基本单位编号 - FBaseUnitId.FNumber
        /// </summary>
        public string FBaseUnitNumber { get; set; }

        /// <summary>
        /// 创建日期 - FCreateDate
        /// </summary>
        public string FCreateDate { get; set; }

        /// <summary>
        /// 修改日期 - FModifyDate
        /// </summary>
        public string FModifyDate { get; set; }

        /// <summary>
        /// 创建人 - FCreatorId.FName
        /// </summary>
        public string FCreatorName { get; set; }

        /// <summary>
        /// 修改人 - FModifierId.FName
        /// </summary>
        public string FModifierName { get; set; }

        /// <summary>
        /// 数据状态 - FDocumentStatus
        /// </summary>
        public string FDocumentStatus { get; set; }

        /// <summary>
        /// 禁用状态 - FForbidStatus
        /// </summary>
        public string FForbidStatus { get; set; }
    }

    /// <summary>
    /// K3Cloud数量查询结果
    /// </summary>
    public class K3CloudCountData
    {
        /// <summary>
        /// 数量
        /// </summary>
        public string count { get; set; }

        /// <summary>
        /// 数量（整型）
        /// </summary>
        public int CountValue => int.TryParse(count, out int result) ? result : 0;
    }
} 