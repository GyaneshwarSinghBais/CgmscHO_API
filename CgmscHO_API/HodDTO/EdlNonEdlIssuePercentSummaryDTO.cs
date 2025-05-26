using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.HodDTO
{
    public class EdlNonEdlIssuePercentSummaryDTO
    {       
        public String? EDLTYPE { get; set; }
        public Int64? MCID { get; set; }
        public String? MCATEGORY { get; set; }
        public Int64? AI { get; set; }
        public Int64? NOSISSUE { get; set; }
        public Double? PER { get; set; }
    }

   
}
