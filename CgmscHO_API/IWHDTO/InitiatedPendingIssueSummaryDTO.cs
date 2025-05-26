using System.ComponentModel.DataAnnotations;
namespace CgmscHO_API.Models
{
    public class InitiatedPendingIssueSummaryDTO
    {
        [Key]

        public Int64? FROMWAREHOUSEID { get; set; }
        public Int64? NOSTOWH { get; set; }
        public Int64? NOSITEMS { get; set; }
        public Int64? TOWHSTOCKOUT { get; set; }
        public Int64? NOSTNO { get; set; }
        public Int64? AVGDAYSDEL { get; set; }
        public string? FROMWAREHOUSENAME { get; set; }


    }
}
