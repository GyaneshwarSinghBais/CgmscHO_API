using System.ComponentModel.DataAnnotations;
namespace CgmscHO_API.QCDTO
{
    public class QCPendingAreaDetails
    {





        [Key]
        public long itemid { get; set; }

        public string? itemcode { get; set; }

        public string? itemtypename { get; set; }

        public string? itemname { get; set; }

        public string? strength1 { get; set; }

        public string? batchno { get; set; }

        public long? noswh { get; set; }

        public long? UQCQTY { get; set; }

        public Decimal? Stockvalue { get; set; }

        public string? WarehouseRecDT { get; set; }

        public string? WHQCIssueDT { get; set; }

        public string? CourierPickDT { get; set; }

        public string? SampleReceiptInHODT { get; set; }

        public string? LABISSUEDATE { get; set; }

        public string? LAbReceiptDT { get; set; }

        public string? HOQCReportRecDT { get; set; }

        public string? LABRESULT { get; set; }

        public long? AnalysisDays { get; set; }
    }
}
