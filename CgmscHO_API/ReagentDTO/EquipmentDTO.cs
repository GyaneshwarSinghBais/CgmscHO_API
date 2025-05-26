using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.Models
{
    public class EquipmentDTO
    {
        [Key]       
        public string? EQPNAME { get; set; }
        public string? MAKE { get; set; }
        public string? MODEL { get; set; }
        public string? MMID { get; set; }
    }
}
