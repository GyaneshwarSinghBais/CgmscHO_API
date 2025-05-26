
using System.ComponentModel.DataAnnotations;


namespace CgmscHO_API.Models
{
    public class RCDetailDTO
    {
        [Key]
        public Int64 itemid { get; set; }
        public string? RCRate { get; set; } 
        public string? RCStartDT { get; set; }
        public string? RCEndDT { get; set; }
        public string? sup { get; set; }
        public string? schemecode { get; set; }
        public string? schemename { get; set; }    

    }
}
