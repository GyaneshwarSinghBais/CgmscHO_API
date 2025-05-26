
using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.Models
{
    public class districtWiseDhsDmeStockDTO
    {
        [Key]
        public Int64 ID { get; set; }
        public Int64 DISTRICTID { get; set; }
        public String? DISTRICTNAME { get; set; }
        public String? EDLITEMCODE { get; set; }
        public Int64? DHSSTOCK { get; set; }
        public Int64? DMESTOCK { get; set; }
    }
}
