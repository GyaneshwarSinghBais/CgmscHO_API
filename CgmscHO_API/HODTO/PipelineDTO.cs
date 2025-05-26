using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.Models
{
    public class PipelineDTO
    {
        [Key]
       
        public Int64? PONOID { get; set; }
        public string? DETAILS { get; set; }
       
    }
}
