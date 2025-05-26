using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.Models
{
    public class InTransitHOtoLabSummaryDTO
    {   
        [Key]
   
        public Int64? ID { get; set; }
        public string? LABNAME { get; set; }
        public Int64? NOSITEM { get; set; }
        public Int64? NOSBATCH { get; set; }
        public Int64? DROPPED { get; set; }
        public Int64? ABOVE15 { get; set; }
        public Int64? BETW6_15 { get; set; }
        public Int64? BET2_5 { get; set; }
        public Int64? TODAY { get; set; }
        public Int64? LABID { get; set; }
        

    }
}
