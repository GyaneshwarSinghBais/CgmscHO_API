namespace CgmscHO_API.TransactionDTO
{
    public class HoldBatchHistoryDTO
    {
        public string? WAREHOUSENAME { get; set; }
        public int? MCID { get; set; }
        public string? CATEGORY { get; set; }
        public string? ITEMCODE { get; set; }
        public string? ITEMNAME { get; set; }
        public string? STRENGTH { get; set; }
        public string? SKU { get; set; }
        public string? BATCHNO { get; set; }
        public DateTime? MFGDATE { get; set; }
        public DateTime? EXPDATE { get; set; }
        public decimal? holdStock { get; set; }
        public string? holddate { get; set; }
        public string? holdreason { get; set; }
        public string? PONO { get; set; }
        public DateTime? PODATE { get; set; }
        public string? SUPPLIERNAME { get; set; }
        public decimal? SupplierReceipt { get; set; }
        public decimal? IWHReceiptQTy { get; set; }
        public decimal? Fac_iss_qty { get; set; }
        public decimal? RF_Qty { get; set; }
        public decimal? rsqty { get; set; }
        public decimal? rpqty { get; set; }
    }
}
