using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.Models
{
    public class masAccYearSettingsModel
    {
        [Key]
        public Int32 ACCYRSETID { get; set; }
        public string? ACCYEAR { get; set; }
        public string? STARTDATE { get; set; }
        public string? ENDDATE { get; set; }
        public string? SHACCYEAR { get; set; }
        public Int32 YEARORDER { get; set; }       
    }
}
