
using System.ComponentModel.DataAnnotations;
namespace CgmscHO_API.Models
{



    public class TransportVoucherDTO
    {
        [Key]

        public Int64? INDENTID { get; set; }
     

        public string? FACILITYNAME { get; set; }
        public string? DISTRICTNAME { get; set; }
        public string? ISSUEVOUCHER { get; set; }
        public string? ISSUEVOUCHERDT { get; set; }
        public string? NOSITEMS { get; set; }
        public string? TRAVELVOUCHERISSUEDT { get; set; }

        public string? INDENTNO { get; set; }

        public string? INDENDT { get; set; }
        public string? DETAILS { get; set; }
        public string? LONGITUDE { get; set; }
        public string? LATITUDE { get; set; }
        public string? TRAVALEID { get; set; }
        public string? FACILITYID { get; set; }
        public string? VEHICALNO { get; set; }


        public string? PLONGITUDE { get; set; }
        public string? PLATITUDE { get; set; }
        public string? ParentFacility { get; set; }
        public string? ParentFacid { get; set; }
        public string? facilitytypeid { get; set; }

        public string? InchargeMobile { get; set; }
        public string? Inchargename { get; set; }
        public string? InchargeDesignation { get; set; }

        public string? ParentInchargeMobile { get; set; }
        public string? ParentInchargename { get; set; }
        public string? ParentInchargeDesignation { get; set; }






    }
}
