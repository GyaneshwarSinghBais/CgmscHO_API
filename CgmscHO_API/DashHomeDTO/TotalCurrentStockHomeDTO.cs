using System.ComponentModel.DataAnnotations;
namespace CgmscHO_API.DashHomeDTO
{
    public class TotalCurrentStockHomeDTO
    {


        [Key]
        public Int64 nositems { get; set; }
        public Decimal STKVALUE { get; set; }
        public Int64 EDLnositems { get; set; }
        public Decimal EDLitemValue { get; set; }
        public Int64 NEDLnositems { get; set; }
        public Decimal NonEDLitemValue { get; set; }
    }
}
