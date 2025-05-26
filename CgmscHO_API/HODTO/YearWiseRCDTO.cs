using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.HODTO
{
    public class YearWiseRCDTO

    {
        [Key]
        public Int32 ACCYRSETID { get; set; }
        public string? ACCYEAR { get; set; }

        public string? EDL { get; set; }
        public string? NONEDL { get; set; }
        public string? ALLRC { get; set; }
    
    }
}
