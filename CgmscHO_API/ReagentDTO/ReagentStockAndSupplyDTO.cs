using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.Models
{
    public class ReagentStockAndSupplyDTO
    {
        [Key]
        public int? SLNO { get; set; }
        public string? FACILITYNAME { get; set; }
        public string? EQPNAME { get; set; }
        public int? NOSREAGENT { get; set; }
        public int? STKAVAILABLE { get; set; }
        public decimal? STOCKVALUECR { get; set; }
        public int? NOSISSUED { get; set; }
        public decimal? NOSISSUEDVALUECR { get; set; }
        public int? NOSFACPIPELINE { get; set; }
        public decimal? NOSFACPIPELINEVALUE { get; set; }
        public int? NOSFACLABISSUED { get; set; }
        public decimal? FACLABISSUEVALUECR { get; set; }        
        public int? NOSFACSTOCK { get; set; }
        public decimal? FACSTOCKVALUECR { get; set; }
        public string STKP { get; set; }
        public int? FACILITYID { get; set; }
        public string? ORDERDP { get; set; }
        public string? EQAVAILABLE { get; set; }
        public string? DISTRICTNAME { get; set; }
        public int? DISTRICTID { get; set; }
        public int? MMID { get; set; }
        public string? FOOTER3 { get; set; }
        public string? FOOTER2 { get; set; }
        public string? CONTACT { get; set; }

    }
}
