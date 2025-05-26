using System.ComponentModel.DataAnnotations;
namespace CgmscHO_API.DashHomeDTO
{
    public class StockHomeDTO
    {

        [Key]
         public Int16 sr { get; set; }
        public string EDLtpe { get; set; }
        public Int64 nositems { get; set; }
        public Decimal? STKVALUE { get; set; }


    }
}
