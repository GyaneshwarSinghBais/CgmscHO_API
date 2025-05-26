using System.ComponentModel.DataAnnotations;


namespace CgmscHO_API.Models
{
    public class FitReportDTO
    {
        [Key]
        public string? FITMONTH { get; set; }
        public string? MONTHNUMBER { get; set; }
        public Int64? COUNTFITFILE { get; set; }
        public Int64? BUDGETID { get; set; }
        public Double? TOBEPAYDVALUE { get; set; }
        public Int64? YR { get; set; }
    }




}
