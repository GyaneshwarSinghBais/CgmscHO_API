using System.ComponentModel.DataAnnotations;
namespace CgmscHO_API.DashHomeDTO
{
    public class StockoutPerHome
    {

         [Key]
        public Int64? EDLtypeid { get; set; }

        public string? EDLtpe { get; set; }


        public Int64? nositems { get; set; }
        public Int64? stockout { get; set; }
        public Int64? stockin { get; set; }
        public Decimal? stockoutp { get; set; }
        
    }
}
