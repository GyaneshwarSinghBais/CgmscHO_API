using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.DashHomeDTO
{
    public class IndentHomeDTO
    {


        [Key]
  
        public Int64 accyrsetid { get; set; }

        public string? ACCYEAR { get; set; }

        public string? SKU { get; set; }

        public Int64? DHSAI { get; set; }
        public Int64? DMEAI { get; set; }
        
    }
}
