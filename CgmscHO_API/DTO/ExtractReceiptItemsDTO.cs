using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.DTO
{
    public class ExtractReceiptItemsDTO
    {
        [Key]
        public Int64 INWNO { get; set; }
        public Int64? ITEMID { get; set; }
        public string? BATCHNO { get; set; }
        public string? EXPDATE { get; set; }
        public Int64? ISSUEBATCHQTY { get; set; }
        public Int64? INDENTITEMID { get; set; }
        public Int64? FACRECEIPTID { get; set; }
        public Int64? FACRECEIPTITEMID { get; set; }
        public string? MFGDATE { get; set; }
        public Int64? PONOID { get; set; }
        public Int32? QASTATUS { get; set; }
        public string? WHISSUEBLOCK { get; set; }
    }
}
