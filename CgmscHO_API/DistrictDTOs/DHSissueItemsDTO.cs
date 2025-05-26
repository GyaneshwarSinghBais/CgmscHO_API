using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.DistrictDTOs
{
    public class DHSissueItemsDTO
    {
        [Key]
        public string? CMHOITEM { get; set; }
        public decimal? CMHOVALUE { get; set; }
        public string? DHITEM { get; set; }
        public decimal? DHVALUELAC { get; set; }
        public string? CHCITEM { get; set; }
        public decimal? CHCVALUE { get; set; }
        public string? OTHERFACITEMS { get; set; }
        public decimal? OTHERFACVALUE { get; set; }
    }
}
