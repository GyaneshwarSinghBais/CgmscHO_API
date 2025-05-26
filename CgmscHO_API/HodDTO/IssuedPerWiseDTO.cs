using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.HodDTO
{
    public class IssuedPerWiseDTO
    {
        [Key]
        public String PERCENTAGE { get; set; }
        public Int64? NOSDRUGS { get; set; }
        public Int64? ORDERDP { get; set; }
    }
   
}
