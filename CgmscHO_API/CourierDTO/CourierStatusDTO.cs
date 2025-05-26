using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.Models
{
    public class CourierStatusDTO
    {
        [Key]   
        public Int64 ID { get; set; }
        public string? STATUSDATE { get; set; }
        public string? STATUS { get; set; }
      

    }
}
