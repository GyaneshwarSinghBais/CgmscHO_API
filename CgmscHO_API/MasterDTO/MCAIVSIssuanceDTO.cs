using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.DTO
{
    public class MCAIVSIssuanceDTO
    {
        [Key]
        public Int64 mcid { get; set; }
        public string? MCATEGORY { get; set; }
        public string? nositems { get; set; }
        public Double? ivalue { get; set; }
        public string? nosissued { get; set; }
        public Double? issueValued { get; set; }
        public string? ReadystkavailableAgainstAI { get; set; }
        public string? NotIssuedOnlyPipeline { get; set; }
        public string? NosBalanceStockAvailable { get; set; }
        public Double? TotalBAlStockValue { get; set; }
        public string? NotLiftedBalanceStock { get; set; }
        public string? concernStockoutButAvailableInOTherWH { get; set; }

        public string? IssuedMorethanAI { get; set; }
        public Double? IssuedMorethanAIExtraVAlue { get; set; }

        public string? TotalNOCTaken { get; set; }
        public Double? TotalNOCValue { get; set; }

        public string? NotIssuedStockOutNOCTaken { get; set; }


        public string? LPOGen { get; set; }
        public Double? lpovalue { get; set; }

    }
}
