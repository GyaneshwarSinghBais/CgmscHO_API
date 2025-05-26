using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.AttendenceDTO
{
    public class GetLocationDTO
    {
        [Key]
        public Int32 LocationId { get; set; }
        public string? LocationName { get; set; }
    }
}
