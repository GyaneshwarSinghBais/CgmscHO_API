using System.ComponentModel.DataAnnotations;
namespace CgmscHO_API.Models
{
    public class NOCPendingDetailsDTO
    {
        [Key]
   
        public Int64? SR { get; set; }
      
        public string? FACILITYNAME { get; set; }
        public string? ITEMCODE { get; set; }
        public string? ITEMNAME { get; set; }
        public string? STRENGTH1 { get; set; }
        public string? UNIT { get; set; }

        public string? CMHOForwardDT { get; set; }
     

        public Int64? APPLIEDQTY { get; set; }
        public Int64? CMHOAPRQTY { get; set; }

        
        public Int64? READYWH { get; set; }
        public Int64? UQCWH { get; set; }

        public Int64? TRANSFERQTY { get; set; }
        public Int64? UQCTotal { get; set; }
        public Int64? TOTALREADYCGMSC { get; set; }
        public string? TDATE { get; set; }
        
        public string? DISTRICTNAME { get; set; }
        public string? NOCNUMBER { get; set; }

        public Int64? NOCID { get; set; }
        public Int64? FACILITYID { get; set; }
        
    }
}
