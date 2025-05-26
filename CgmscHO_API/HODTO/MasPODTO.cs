using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.Models
{
    public class MasPODTO
    {
        [Key]
        public Int64? PONOID { get; set; }
        public string? NAME { get; set; }
        
      
       
    }
}
