namespace CgmscHO_API.HODTO
{
    public class StockOutDetail1DTO
    {
        public string? WAREHOUSEID { get; set; }
        public string? WAREHOUSENAME { get; set; }
        public string? ITEMID { get; set; }
        public string? ITEMCODE { get; set; }
        public string? ITEMNAME { get; set; }
        public string? STRENGTH1 { get; set; }
        public string? EDLType { get; set; }
        public decimal? READYFORISSUE { get; set; }
        public decimal? PENDING { get; set; }
        public decimal? StockOut { get; set; }
        public decimal? StockIn { get; set; }
        public decimal? StockOutIWHPipe { get; set; }
        public decimal? StockOutPoPipe { get; set; }
    }
}
