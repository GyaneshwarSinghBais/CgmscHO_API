using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.DTO
{
    public class ReceiptDetailsDTO
    {        
        [Key]
        public string BATCHNO { get; set; }
        public string? ITEMCODE { get; set; }
        public string? ITEMNAME { get; set; }
        public string? STRENGTH1 { get; set; }
        public Int64? ABSRQTY { get; set; }
        public string? EXPDATE { get; set; }
        public string? LOCATIONNO { get; set; }
        public string? STOCKLOCATION { get; set; }
        public Int64? INWNO { get; set; }
        public Int64? ITEMID { get; set; }
        public Int64? FACRECEIPTID { get; set; }
        public Int64? FACRECEIPTITEMID { get; set; }

    }
}
