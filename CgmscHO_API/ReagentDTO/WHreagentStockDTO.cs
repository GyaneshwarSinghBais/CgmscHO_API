using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.Models
{
    public class WHreagentStockDTO
    {
        [Key]
        public int SERIAL_NO { get; set; }
        public string? EQPNAME { get; set; }
        public int? REQUIRED { get; set; }
        public string? WAREHOUSENAME { get; set; }
        public int? NOS { get; set; }
        public decimal? STOCKVALUE { get; set; }
        public int? MMID { get; set; }
        public int? NOSWH { get; set; }     

    }
}
