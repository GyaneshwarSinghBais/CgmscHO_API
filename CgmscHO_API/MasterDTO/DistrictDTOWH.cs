using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.Models
{
    public class DistrictDTOWH
    {
        
        [Key]
        public Int64? DISTRICTID { get; set; }
        public string? DISTRICTNAME { get; set; }
        public string? DISTNAME { get; set; }
        public string? WAREHOUSEID { get; set; }
        
        public Int64? USERID { get; set; }
        public string? EMAILID { get; set; }
        
    }
}
