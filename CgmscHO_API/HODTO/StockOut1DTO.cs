namespace CgmscHO_API.HODTO
{
    public class StockOut1DTO
    {
        public string? EDLType { get; set; }
        public Int32? warehouseid { get; set; }
        public string? warehousename { get; set; }
        public Int32? noofitems { get; set; }
        public Int32? stockout { get; set; }
        public Int32? STOCKOUTIWHPIPE { get; set; }
        public Int32? STOCKOUTPOPIPE { get; set; }
        public Int32? stockin { get; set; }
        public Int32? STOCKINIWHPIPE { get; set; }
        public Int32? STOCKINPOPIPE { get; set; }
        public Double? Percentage { get; set; }
        
    }
}
