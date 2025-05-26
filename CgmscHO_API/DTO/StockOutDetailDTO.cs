using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.DTO
{
    public class StockOutDetailDTO
    {
        [Key]
        public string? ITEMCODE { get; set; }
        public string? EDL { get; set; }
        public string? ITEMNAME { get; set; }
        public string? STRENGTH1 { get; set; }
        public string? STOCKINWH { get; set; }
    }
}
