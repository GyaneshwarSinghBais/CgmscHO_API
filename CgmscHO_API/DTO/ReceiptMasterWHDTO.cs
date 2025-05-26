using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.DTO
{
    public class ReceiptMasterWHDTO
    {

        [Key]
        public Int64 NOCID { get; set; }
        public string? REQDATE { get; set; }
        public string? REQNO { get; set; }

        public string? NOSITEMSREQ { get; set; }
        public string? WHISSUENO { get; set; }
        public string? WHISSUEDT { get; set; }
        public string? FACRECEIPTNO { get; set; }
        public string? FACRECEIPTDATE { get; set; }
        public string? RSTATUS { get; set; }


        public Int64? NOSITEMSISSUED { get; set; }
        public Int64? INDENTID { get; set; }
        public Int64? FACRECEIPTID { get; set; }
        public Int64? FACILITYID { get; set; }
        public Int64? WAREHOUSEID { get; set; }



    }
}
