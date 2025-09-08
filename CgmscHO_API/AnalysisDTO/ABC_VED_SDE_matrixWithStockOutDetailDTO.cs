namespace CgmscHO_API.AnalysisDTO
{
    public class ABC_VED_SDE_matrixWithStockOutDetailDTO
    {
        public Int32? ITEM_ID { get; set; }
        public string? ITEMCODE { get; set; }
        public string? DRUG_NAME { get; set; }
        public string? STRENGTH1 { get; set; }
        public string? UNIT { get; set; }
        public string? ITEMTYPENAME { get; set; }
        public string? EDLTYPE { get; set; }
        public string? EDLCAT { get; set; }
        public string? MCATEGORY { get; set; }
        public Int32? mcid { get; set; }
        public decimal? ORDER_VALUE { get; set; }
        public Int32? READYWTOCK { get; set; }
        public Int32? UQCSTOCK { get; set; }
        public Int32? SUPPLIERPIPELINE { get; set; }
        public Int32? transferQTY { get; set; }

        public string? StockOut { get; set; }
        public string? StockIn { get; set; }
        public string? StockOutPoPipe { get; set; }
        public Int32? StockOutPoQty { get; set; }

        public string? RCValid { get; set; }
        public string? RCNotValid { get; set; }
        public string? Pricecnt { get; set; }
        public string? Evalutioncnt { get; set; }
        public string? LiveCnt { get; set; }
        public string? Rentendercn { get; set; }

        public string? abc_category { get; set; }
        public string? vedcat { get; set; }
        public string? sde_class { get; set; }

        public string? ABC_VED_SDE_CATEGORY { get; set; }
    }
}
