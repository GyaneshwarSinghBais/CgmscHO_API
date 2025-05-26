using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.Models
{
    public class LabIssuePendingSummary
    {   
        [Key]
  
        public Int64? ID { get; set; }
        public string? MCID { get; set; }
        public string? MCATEGORY { get; set; }
        public string? NOUSBATCH { get; set; }       
        public string? NOSITEMS { get; set; }
        public Int64? DELAYPARA1 { get; set; }
        public string? DELAYPARA { get; set; }


    }
}
