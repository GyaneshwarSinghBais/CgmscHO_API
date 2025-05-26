using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.QCDTO
{
    public class QCHoldItemDetails
    {
        [Key]
        public string? ID { get; set; }

        public long itemid { get; set; }

        public string? itemcode { get; set; }

        public string? itemname { get; set; }

        public string? unit { get; set; }

        public string? strength1 { get; set; }

        public string? pono { get; set; }

        public string? soissuedate { get; set; }

        public string? suppliername { get; set; }

        public string? batchno { get; set; }

        public Decimal? finalrategst { get; set; }

        public Decimal? Stkvaluelacs { get; set; }

        public long? rqty { get; set; }

        public long? allotqty { get; set; }

        public long? STK { get; set; }

        public string? mcategory { get; set; }

        public string? HOLDREASON { get; set; }

        public string? HOLDDATE { get; set; }

        public long ponoid { get; set; }
    }
}
