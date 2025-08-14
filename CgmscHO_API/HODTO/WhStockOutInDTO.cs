namespace CgmscHO_API.HODTO
{
    public class WhStockOutInDTO
    {
        public string? warehouseid { get; set; }
        public string? warehousename { get; set; }
        public Int32? noofitems { get; set; }
        public decimal? stockout { get; set; }
        public decimal? stockin { get; set; }


    }
}
