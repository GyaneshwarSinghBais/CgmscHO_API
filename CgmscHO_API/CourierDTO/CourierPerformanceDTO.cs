using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.Models
{
    public class CourierPerformanceDTO
    {
        [Key]   
        public Int64 WAREHOUSEID { get; set; }
        public string? WAREHOUSENAME { get; set; }
        public string? NOOFDOCKET { get; set; }
        public Int64? NOOFITEMS { get; set; }
        public Int64? DAYSTAKEN { get; set; }
        public Int64? AVGDAYS { get; set; }


    }
}
