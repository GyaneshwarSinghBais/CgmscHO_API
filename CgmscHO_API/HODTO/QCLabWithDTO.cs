using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.Models
{
    public class QCLabWithDTO
    {
        [Key]
        public string? ID { get; set; }
        
        public Int64? MCID { get; set; }
        public string? MCATEGORY { get; set; }
        public Int64? NOSITEMS { get; set; }
        public Int64? NOSBATCH { get; set; }
        
        public Double? UQCVALUE { get; set; }
    
    
        public string? TIMELINE { get; set; }
        public string? EXCEDDEDSINCETIMELINE { get; set; }
        public string? EXCEDDEDSINCETIMELINE1 { get; set; }

    }
}
