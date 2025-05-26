using System.ComponentModel.DataAnnotations;
namespace CgmscHO_API.Models
{
    public class FACwiseStockPostionDTO
    {
        [Key]






        public Int64? ITEMID { get; set; }
        
        public Int64? FACILITYID { get; set; }
     

        public string? FACILITYNAME { get; set; }

        public string? itemtypename { get; set; }

        public string? GROUPNAME { get; set; }

        public string? ITEMCODE { get; set; }
        public string? ITEMNAME { get; set; }
        public string? STRENGTH1 { get; set; }
        public string? EDL { get; set; }
        

        public Int64? FACSTOCK { get; set; }
        
        public Int64? CMHOSTK { get; set; }
        public Int64? WHREADY { get; set; }
        public Int64? WHPENDING { get; set; }

        public Int64? INDENTQTYPENDING { get; set; }

        public Int64? WHISSUEPENDING_L180 { get; set; }

        public Int64? BALIFT_L180 { get; set; }

        public Int64? BALLP_L180 { get; set; }
        public Int64? BALNOCAFTERLPO { get; set; }

    
        public Int64? AIQTY { get; set; }
       


    }
}
