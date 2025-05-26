using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.DTO
{
    public class FacMonthIndentDTO
    {
        [Key]
        public Int64 NOCID { get; set; }
        public Int32? NOSITEMSREQ { get; set; }
        public Int32? NOSISSUED { get; set; }
        public Int32?  WAREHOUSEID { get; set; }
        public string? ReqDate { get; set; }
        public string? REQNO { get; set; }
        public string? WHISSUEDT { get; set; }
        public string? ISTATUS { get; set; }
        public Int64? INDENTID { get; set; }
        public Int64? FACRECEIPTID { get; set; }

        
            
            

    }
}
