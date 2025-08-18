namespace CgmscHO_API.AnalysisDTO
{
    public class ABCanalysisSummaryDetailDTO
    {
        public Int32? ITEM_ID { get; set; }
        // public string? ITEMCODE { get; set; }
        public string? DRUG_NAME { get; set; }
        public string? STRENGTH1 { get; set; }
        public string? UNIT { get; set; }
        public string? ITEMTYPENAME { get; set; }
        public string? EDLCAT { get; set; }
        public string? RCStatus { get; set; }
        public string? RCENDDate { get; set; }
        public Int32? rcremainingdays { get; set; }
        public Int32? CNTSUP { get; set; }
        public string? tenderstatus { get; set; }
        public Int32? READYFORISSUE { get; set; }
        public Int32? PENDING { get; set; }
        public Int32? iwhPipeline { get; set; }
        public Int32? SupplierPipeline { get; set; }
        public decimal? ORDER_VALUE { get; set; }
        public decimal? CUMULATIVE_VALUE { get; set; }
        public decimal? CUMULATIVE_PERCENT { get; set; }
        public string? ABC_CATEGORY { get; set; }

        // 🔽 Newly Added Properties
        public string? Pricecnt { get; set; }
        public string? Evalutioncnt { get; set; }
        public string? LiveCnt { get; set; }
        public string? Rentendercn { get; set; }

       
    }
}
