
using System.ComponentModel.DataAnnotations;
namespace CgmscHO_API.DistrictDTO
{
    public class DistrictStkCount
    {
     
        [Key]

        public Int64? DISTRICTID { get; set; }
        public String? DISTRICTNAME { get; set; }


        public Int64? EDL { get; set; }
        public Int64? NEDL { get; set; }
        public Int64? TOTAL { get; set; }
        
    }
}
