using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.DTO
{
    public class GetOpeningStocksReportDTO
    {
        [Key]
        public Int64 RACKID { get; set; }
        public String? LOCATIONNO { get; set; }
    }
}
