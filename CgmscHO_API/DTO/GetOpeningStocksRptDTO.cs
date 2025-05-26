using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.DTO
{
    public class GetOpeningStocksRptDTO
    {
        [Key]
        public Int64 ID { get; set; }
        public Int64? INWNO { get; set; }
        public Int64? OPENING_QTY { get; set; }
        public String? RECEIPTDATE { get; set; }
        public Int64? FACILITYID { get; set; }
        public Int64? ITEMID { get; set; }
        public String? ITEMCODE { get; set; }
        public String? UNIT { get; set; }
        public String? STRENGTH1 { get; set; }
        public String? ITEMNAME { get; set; }
        public String? BATCHNO { get; set; }
        public String? MFGDATE { get; set; }
        public String? EXPDATE { get; set; }
        public String? ABSRQTY { get; set; }
        public Int64? RECEIPTID { get; set; }
        public Int64? RECEIPTITEMID { get; set; }
    }
}


