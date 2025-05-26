using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.Models
{
    public class FinalStatusPendingInHOQCDTO
    {   
        [Key]
        
        public Int64? ID { get; set; }
        public Int64? MCID { get; set; }
        public string? MCATEGORY { get; set; }       
        public Int64? NOOFDAYS { get; set; }
        public Int64? NOOFITEMS { get; set; }
        public Int64? NOOFSAMPLES { get; set; }
       // public Int64? NOOFDOCKETS { get; set; }
        public Int64? NOOFLABS { get; set; }
        

    }
}
