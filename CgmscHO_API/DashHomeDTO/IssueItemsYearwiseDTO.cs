using System.ComponentModel.DataAnnotations;
namespace CgmscHO_API.DashHomeDTO
{
    public class IssueItemsYearwiseDTO
    {
        [Key]
        public Int64 yrid { get; set; }
        public string? ACCYEAR { get; set; }
        public Int64 DHSQTY { get; set; }
        public Int64 DMEQTY { get; set; }  
        public Decimal? DHSValuelacs { get; set; }
        public Decimal? DMEValuelacs { get; set; }
        public Int64? mcid { get; set; }

    }
}
