using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.DTO
{
    public class StockitemsDTO
    {
        [Key]
        public Int32 ID { get; set; }
        public string? DETAILS { get; set; }
    }
}
