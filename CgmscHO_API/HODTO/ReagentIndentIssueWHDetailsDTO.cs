using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.Models
{
    public class ReagentIndentIssueWHDetailsDTO
    {
             [Key]
        public Int64? NOCID { get; set; }
        public Int64? MMID { get; set; }

        public string? WAREHOUSENAME { get; set; }
        public string? DISTRICTNAME { get; set; }
        public string? FACILITYNAME { get; set; }
        public string? EQPNAME { get; set; }
        public string? MAKE { get; set; }
        public string? MODEL { get; set; }
        public string? INDENTDT { get; set; }
        public string? WHISSUEDATE { get; set; }

        public Int64? NOSITEMS { get; set; }
        public Double? INDENTVALUE { get; set; }
      
        
       


    }
}
