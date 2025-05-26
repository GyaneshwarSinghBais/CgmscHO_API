using System.ComponentModel.DataAnnotations;
namespace CgmscHO_API.MasterDTO
{
    public class FacilityInfoAamDTO
    {
        [Key]
        public Int64? USERID { get; set; }
        public Int64? FACILITYID { get; set; }
        public String? DISTRICTNAME { get; set; }
        public String? LOCATIONNAME { get; set; }
        public String? FACILITYNAME { get; set; }
        public String? PARENTFAC { get; set; }
        public String? LONGITUDE { get; set; }
        public String? LATITUDE { get; set; }
        public String? PHONE1 { get; set; }
        public String? MFACILITY { get; set; }
        public String? CONTACTPERSONNAME { get; set; }
        public String? FACILITYCODE { get; set; }
        public Int64? DISTRICTID { get; set; }
        public String? FACILITYTYPECODE { get; set; }
        public String? FACILITYTYPEDESC { get; set; }
        public Int64? ELDCAT { get; set; }
        public Int64? WAREHOUSEID { get; set; }
        public String? WAREHOUSENAME { get; set; }
        public String? WHEMAIL { get; set; }
        public String? WHCONTACT { get; set; }

        public String? EMAILID { get; set; }
        public String? INDENTDURATION { get; set; }
    }
}
