using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.AttendenceDTO
{
    public class GetDesignationDTO
    {
        [Key]
        public int DesignationId { get; set; }
        public string DesignationsName { get; set; }
    }
}
