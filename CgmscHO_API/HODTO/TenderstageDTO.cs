using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.Models
{

    public class TenderstageDTO
    {
        [Key]
        public Int64? SCHEMEID { get; set; }
        public string? CATEGORYNAME { get; set; }
        public string? SCHEMECODE { get; set; }
        public string? SCHEMENAME { get; set; }
        public string? EPROCNO { get; set; }
        public string? STARTDT { get; set; }
        public string? ACTCLOSINGDT { get; set; }
        public string? COV_A_OPDATE { get; set; }
        public string? DA_DATE { get; set; }
        public string? COV_B_OPDATE { get; set; }
        public string? PRICEBIDDATE { get; set; }
        public string? PREBIDENDDT { get; set; }
        
        public string? WEBID { get; set; }
        
        public Int64? NOOFITEMS { get; set; }
        public Int64? ITEMAEDL { get; set; }
        public Int64? NOOF_BID_A { get; set; }
        public Int64? NOOFITEMSCOUNTA { get; set; }

        public Int64? NOOFITEMSCOUNTAEDL { get; set; }
        public string? PRICENOTACCPT_REJECT { get; set; }
        public string? STATUS { get; set; }
        public string? REMARKSDATA { get; set; }
        





    }
}
