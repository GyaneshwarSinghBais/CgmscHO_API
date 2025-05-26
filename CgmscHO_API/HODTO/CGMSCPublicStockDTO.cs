
using System.ComponentModel.DataAnnotations;
using System.IO.Pipelines;

namespace CgmscHO_API.Models
{

    public class CGMSCPublicStockDTO
    {
        [Key]
        public Int64 id { get; set; }
        public string? name { get; set; }
        public string? details { get; set; }
        public string? edltype { get; set; } 
    }
}
