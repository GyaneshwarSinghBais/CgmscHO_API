using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.Models
{
    public class TotalRCDTO
    {
        [Key]
     
        public Int64? MCID { get; set; }
        public string? MCATEGORY { get; set; }
        public Int64? EDL { get; set; }
        public Int64? NEDL { get; set; }
        public Int64? TOTAL { get; set; }




    }
}
