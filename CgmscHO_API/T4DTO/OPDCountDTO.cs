

using System.ComponentModel.DataAnnotations;
using System.IO;

namespace CgmscHO_API.Models
{
    public class OPDCountDTO
    {
        [Key]
   
        public Int64? FACILITYID { get; set; }
     

        public string? FACILITYNAME { get; set; }
        public Int64? NOSFAC { get; set; }
        
        public Int64? NOITEMID { get; set; }
        public Int64? PATNO { get; set; }
        public Double? NOSDR { get; set; }
      


    }
}
