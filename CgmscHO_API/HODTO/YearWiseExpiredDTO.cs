using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.Models
{
    public class YearWiseExpiredDTO    {
        
        [Key]
        public string? YEAR { get; set; }
        public Int64? MCID { get; set; }
        public string? MCATEGORY { get; set; }
        public string? NOOFITEMS { get; set; }
        public Double? EXPVALUEBASIC { get; set; }
        public Double? EXPVALUEGST { get; set; }
    }
}


