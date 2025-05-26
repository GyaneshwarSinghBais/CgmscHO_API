using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.Models
{
    public class DistrictWiseStockDTO
    {
        [Key]
        public Int64 DISTRICTID { get; set; }
        public string? DISTRICTNAME { get; set; }
        public Int64? CNTEDL { get; set; }
        public Int64? CNTNONEDL { get; set; }
    }
}
