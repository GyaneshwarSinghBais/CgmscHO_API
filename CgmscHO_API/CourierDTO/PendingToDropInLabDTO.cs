using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.Models
{
    public class PendingToDropInLabDTO
    {
        [Key]   
        public Int64 ID { get; set; }
        public string? DOCKETNO { get; set; }
        public string? LABNAME { get; set; }
        public Int64? DAYS_SINCE_PICKDATE { get; set; }    
    }
}
