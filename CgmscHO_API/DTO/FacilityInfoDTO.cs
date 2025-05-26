using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.DTO
{
    public class FacilityInfoDTO
    {
        [Key]
        public Int64 FACILITYID { get; set; }
        public string? FACILITYCODE { get; set; }
        public string? FACILITYNAME { get; set; }
        public string? FOOTER1 { get; set; }
        public string? FOOTER2 { get; set; }
        public string? FOOTER3 { get; set; }
        public string? EMAIL { get; set; }

        public Int64? DISTRICTID { get; set; }
        public string? DISTRICTNAME { get; set; }
        public string? FACILITYTYPECODE { get; set; }
        public string? FACILITYTYPEDESC { get; set; }

        public string? WAREHOUSENAME { get; set; }
        public string? WHEMAIL { get; set; }

        public string? WHCONTACT { get; set; }

        public Int64? ELDCAT { get; set; }
        public Int64? WAREHOUSEID { get; set; }
        public Int64? FACILITYTYPEID { get; set; }
        public Int32? HODID { get; set; }
        public Int32? CMHOFACILITY { get; set; }
        


    }
}
