using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.Models
{
    public class PendingToSendToLabDTO
    {   
        [Key]

        public Int64? ID { get; set; }
        public string? MCID { get; set; }
        public string? MCATEGORY { get; set; }
        public string? NOOFDAYS { get; set; }       
        public string? NOOFITEMS { get; set; }
        public Int64? NOOFSAMPLES { get; set; }
        public string? NOOFDOCKETS { get; set; }
        public string? NOOFLABS { get; set; }

    }
}
