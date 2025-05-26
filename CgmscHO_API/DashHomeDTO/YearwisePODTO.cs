using System.ComponentModel.DataAnnotations;
namespace CgmscHO_API.DashHomeDTO
{
    public class YearwisePODTO
    {


        [Key]

        public Int64 ACCYRSETID { get; set; }

        public string? SHACCYEAR { get; set; }
        public Int64? dhspoqty { get; set; }
        public Decimal? dhspovalue { get; set; }
        public Int64? DMEPOQTY { get; set; }
        public Decimal? DMEPOVALUE { get; set; }
        public Int64? DHSRQTY { get; set; }
        public Decimal? DHSRVALUE { get; set; }

        public Int64? DMERQTY { get; set; }
        public Decimal? DMERVALUE { get; set; }
        public Int64? TotalPQTY { get; set; }
        public Decimal? TotalPOvalue { get; set; }
        public Int64? TotalRQTY { get; set; }
        public Decimal? TotalRecvalue { get; set; }
    }
}
