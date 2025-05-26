using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.Models
{
    public class WHIndentPendingReagentDTO
    {
      
               [Key]
        public Int32? NOCID { get; set; }

        public string? WAREHOUSENAME { get; set; }
        public string? DISTRICTNAME { get; set; }
        public string? FACILITYNAME { get; set; }

        public string? EQPNAME { get; set; }
        public string? MAKE { get; set; }
        public string? MODEL { get; set; }
        public Int64? NOSITEMS { get; set; }
        public Double? INDENTVALUE { get; set; }
        public string? INDENTDT { get; set; }
        public string? NOCNUMBER { get; set; }
        
       


    }
}
