using System.ComponentModel.DataAnnotations;
namespace CgmscHO_API.Models
{
    public class facddlDTO
    {
        [Key]

        public Int64? FACILITYID { get; set; }
        public Int64? DISTRICTID { get; set; }

        public string? FACILITYNAME { get; set; }
       

    }
}
