using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.Models
{
    public class NearExpDTO
    {
        
        [Key]
        public string? EXPIRYMONTH { get; set; }
        public string? EXPIRYMONTH1 { get; set; }
        public Int64? NOOFITEMS { get; set; }
        public Int64? NOOFBATCHES { get; set; }
        public Double? NEAREXPVALUE { get; set; }

    }
}


