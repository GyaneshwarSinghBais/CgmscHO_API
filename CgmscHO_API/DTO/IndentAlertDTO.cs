using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.Models
{
    public class IndentAlertNewDTO
    {
        [Key]
        public Int64? ITEMID { get; set; }
        public string? ITEMCODE { get; set; }
        public string? EDLCATNAME { get; set; }
        public string? ITEMTYPENAME { get; set; }
        public string? EDL { get; set; }
        

        public string? ITEMNAME { get; set; }
        public string? STRENGTH1 { get; set; }
        

      

        public Int64? AIFACILITY { get; set; }

        public Int64? APPROVEDAICMHO { get; set; }
        public Int64? FACREQFOR3MONTH { get; set; }
        public Int64? FACSTOCK { get; set; }
        public Int64? REQUIREDQTY { get; set; }
        public Int64? WISSUEQTYYEAR { get; set; }
        public Int64? WHREADY { get; set; }

        public string? REMARKS { get; set; }

        public string? EDLCAT { get; set; }

        public Int32? ITEMTYPEID { get; set; }
        public Int64? UNITCOUNT { get; set; }
        public Int64? BALAI { get; set; }

        
            




    }
}
