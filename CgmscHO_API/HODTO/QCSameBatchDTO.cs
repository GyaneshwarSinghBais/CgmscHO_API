using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.Models
{
    public class QCSameBatchDTO    {
        
        [Key]
        public Int16? MCID { get; set; }
        public string? MCATEGORY { get; set; }
        public string? NOSITEM { get; set; }
        public string? NOSBATCHES { get; set; }
    }
}
