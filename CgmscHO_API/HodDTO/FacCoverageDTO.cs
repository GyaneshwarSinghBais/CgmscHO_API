
using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.HodDTO
{
    public class FacCoverageDTO
    {
     

        [Key]
         public Int64? FACILITYTYPEID { get; set; }
        public String FACILITYTYPECODE { get; set; }
        public Int64? NOSFAC { get; set; }

        public String FACILITYTYPEDESC { get; set; }

        public Int64? nositems { get; set; }

        public Int64? nosindent { get; set; }

    }
   
}
