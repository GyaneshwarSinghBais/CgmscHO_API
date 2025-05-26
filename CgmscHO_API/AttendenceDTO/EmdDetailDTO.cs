using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.AttendenceDTO
{
    public class EmdDetailDTO
    {
        [Key]
        public int? EmployeeId { get; set; }
        public string? EmployeeName { get; set; }
        public string? EmployeeCode { get; set; }
        public string? Gender { get; set; }
        public string? ContactNo { get; set; }
        public string? DesignationsName { get; set; }
        public string? DepartmentFName { get; set; }
    }
}
