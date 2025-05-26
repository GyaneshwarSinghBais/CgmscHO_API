using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.DistrictDTOs
{
    public class DMEissueItemsDTO
    {
        [Key]
        public int? FACILITYID { get; set; }
        public string? FACILITYNAME { get; set; }
        public int? NOSISSUEITEMS { get; set; }
        public decimal? FACVALUE { get; set; }
        public int? FACILITYTYPEID { get; set; }
    }
}
