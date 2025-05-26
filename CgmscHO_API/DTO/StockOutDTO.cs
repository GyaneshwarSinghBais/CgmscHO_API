using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.DTO
{
    public class StockOutDTO
    {
        [Key]
        public Int32 MCID { get; set; }
        public string? MCATEGORY { get; set; }
        public Int32 TOTALNOS { get; set; }
        public Int32 STOCKOUT { get; set; }
        public Int32 STOCKINWH { get; set; }
        public Double STOCKOUTPER { get; set; }
    }
}
