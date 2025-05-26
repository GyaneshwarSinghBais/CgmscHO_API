using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.DTO
{
    public class FacilityMCDTO
    {

        [Key]
        public Int64 FACILITYID { get; set; }
        public string? FACILITYNAME { get; set; }
        public string? SIDESIG { get; set; }
        public string? SINAME { get; set; }
        public string? SIMOBILE { get; set; }
        public string? HOSPITALINCHARGE { get; set; }
        public string? HIMOBILE { get; set; }
        
        public string? EMAIL { get; set; }
        public string? USERID { get; set; }
        public Int64? WAREHOUSEID { get; set; }
    
        


    }
}
