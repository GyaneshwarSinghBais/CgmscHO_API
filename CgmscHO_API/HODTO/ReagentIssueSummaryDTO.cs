using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.Models
{
    public class ReagentIssueSummaryDTO
    {
     
        [Key]
        public Int32? MMID { get; set; }


        public string? EQPNAME { get; set; }
        public string? MAKE { get; set; }
        public string? MODEL { get; set; }
        public Int64? NOSREAGENT { get; set; }
        public Int64? NOSFAC { get; set; }
        public Double? ISSUEVALUESINCE3SEP { get; set; }
      
        
       


    }
}
