using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.FacilityDTO
{
    public class FacilityIssueDateWiseDTO
    {
       

        public Int64? FACILITYID { get; set; }
        public Int64? ITEMID { get; set; }
        public Int64? INWNO { get; set; }
        public Int64? ISSUEQTY { get; set; }
        public string? ITEMCODE { get; set; }
        public string? ITEMNAME { get; set; }
        public string? STRENGTH1 { get; set; }
        public string? WARDNAME { get; set; }
        public string? ISSUEDDATE { get; set; }
        public string? DISTRICTID { get; set; }
    }
}
