using System.ComponentModel.DataAnnotations;
namespace CgmscHO_API.Models
{
    public class NOCPendingSummaryDTO
    {
        [Key]

        public Int64? NOCID { get; set; }
        public Int64? FACILITYID { get; set; }
        public Int64? NOSITEMS { get; set; }

        public string? NOCNUMBER { get; set; }
        public string? CMHOFORWARDDT { get; set; }
        public string? FACILITYNAME { get; set; }
        public string? DISTRICTNAME { get; set; }

    }
}
