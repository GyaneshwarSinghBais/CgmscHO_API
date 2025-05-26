using System.ComponentModel.DataAnnotations;
namespace CgmscHO_API.Models
{
    public class NOCApprovedSummaryDTO
    {
        [Key]

        public Int64? FACILITYID { get; set; }
        public Int64? NOSAPPLIED { get; set; }
        public Int64? APPROVED { get; set; }
        public Int64? REJECTED { get; set; }
        public string? FACILITYNAME { get; set; }
        public string? DISTRICTNAME { get; set; }

    }
}
