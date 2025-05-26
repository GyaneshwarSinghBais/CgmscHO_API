using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.Models
{
    public class WHDTO
    {
        [Key]
        public Int64? WAREHOUSEID { get; set; }
        public string? WAREHOUSENAME { get; set; }
        public string? ZONEID { get; set; }
        public Int64? USERID { get; set; }
        public string? ONLYWHNAME { get; set; }
        




    }
}
