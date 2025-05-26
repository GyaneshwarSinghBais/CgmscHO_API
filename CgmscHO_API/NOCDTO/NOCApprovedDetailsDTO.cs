using System.ComponentModel.DataAnnotations;
namespace CgmscHO_API.Models
{
    public class NOCApprovedDetailsDTO
    {
        [Key]

        public Int64? SR { get; set; }
      
        public string? FACILITYNAME { get; set; }
        public string? ITEMCODE { get; set; }
        public string? ITEMNAME { get; set; }
        public string? STRENGTH1 { get; set; }
        public string? UNIT { get; set; }
        public string? NOCDATE { get; set; }
        public string? APPLIEDDT { get; set; }
     

        public Int64? APPLIEDQTY { get; set; }
        public Int64? CMHOAPRQTY { get; set; }
        public string? CMHOAPRDTTIME { get; set; }
        
        public Int64? APPROVEDQTY { get; set; }
        public Int64? REJECTQTY { get; set; }
        
        public string? CGMSCAPRDTTIME { get; set; }
        public string? CGMSCLREMARKS { get; set; }
        public string? ISIWH { get; set; }
        public string? WHNAME { get; set; }
        public string? NOCNUMBER { get; set; }

        public Int64? NOCID { get; set; }
        
    }
}
