using System;

namespace HDPro.CY.Order.Models.OrderCycleBaseDtos
{
    /// <summary>
    /// 调用阀门规则服务的入参 DTO
    /// </summary>
    public class OrderRuleDto
    {
        public int Id { get; set; }

        public DateTime? OrderApprovedDate { get; set; }

        public DateTime? ReplyDeliveryDate { get; set; }

        public DateTime? RequestedDeliveryDate { get; set; }

        public string InnerMaterial { get; set; }

        public string FlangeConnection { get; set; }

        public string BonnetForm { get; set; }

        public string FlowCharacteristic { get; set; }

        public string Actuator { get; set; }

        public string OutsourcedValveBody { get; set; }

        public string ValveCategory { get; set; }

        public string SealFaceForm { get; set; }

        public string SpecialProduct { get; set; }

        public string PurchaseFlag { get; set; }

        public string ProductName { get; set; }

        public string NominalDiameter { get; set; }

        public string NominalPressure { get; set; }
    }
}
