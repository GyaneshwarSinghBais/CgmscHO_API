using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.DTO
{
    public class ReceiptMasterDTO
    {
        [Key]
        public Int64 FACRECEIPTID { get; set; }
        public Int64 NOCID { get; set; }
        public Int64 INDENTID { get; set; }

        public string? REQNO { get; set; }
        public string? REQDATE { get; set; }

        public string? WHISSUENO { get; set; }
        public string? WHISSUEDT { get; set; }

        public string? FACRECEIPTNO { get; set; }
        public string? FACRECEIPTDATE { get; set; }

    }
}
