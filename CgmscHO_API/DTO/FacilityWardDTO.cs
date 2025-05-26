using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.DTO
{
    public class FacilityWardDTO
    {
        [Key]
        public int WardID { get; set; }
        public string? WardName { get; set; }
      
    }
}
