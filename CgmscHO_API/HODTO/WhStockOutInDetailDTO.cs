namespace CgmscHO_API.HODTO
{
    public class WhStockOutInDetailDTO
    {
        public string? warehouseid { get; set; }
        public string? warehousename { get; set; }
        public string? itemcode { get; set; }
        public string? itemname { get; set; }
        public string? strength { get; set; }
        public string? sku { get; set; }
       // public decimal? stock { get; set; }
        public Int32? itemid { get; set; }
        public Int32? READYFORISSUE { get; set; }
        public Int32? PENDING { get; set; }
        public Int32? StockOut { get; set; }
        public Int32? StockIn { get; set; }
        public Int32? iwhPipeline { get; set; }
        public Int32? SupplierPipeline { get; set; }



    }
}
