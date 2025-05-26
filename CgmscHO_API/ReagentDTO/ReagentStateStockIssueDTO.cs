using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.Models
{
    public class ReagentStateStockIssueDTO
    {
        [Key]
        public Int64 MMID { get; set; }
        public string? EQPNAME { get; set; }
      
        public Int64? NOSREAGENT { get; set; }
        public Int64? STKAVAILABLE { get; set; }
        public Double? STOCKVALUECR { get; set; }
        public Int64? NOSWH { get; set; }

        public Int64? NOSISSUED { get; set; }
        public Double? NOSISSUEDVALUECR { get; set; }
        public Int64? NOSFACPIPELINE { get; set; }
        public Double? NOSFACPIPELINEVALUE { get; set; }
        public Int64? NOSFACLABISSUED { get; set; }
        public Double? FACLABISSUEVALUECR { get; set; }

        public Int64? NOSFACSTOCK { get; set; }
        public Double? FACSTOCKVALUECR { get; set; }

    }
}
