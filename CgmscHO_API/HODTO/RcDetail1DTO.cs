namespace CgmscHO_API.HODTO
{
    public class RcDetail1DTO
    {
        public string? itemcode { get; set; }
        public string? itemname { get; set; }
        public string? strength1 { get; set; }
        public string? unit { get; set; }
        public string? suppliername { get; set; }
        public decimal? basicrate { get; set; }
        public decimal? GST { get; set; }
        public decimal? finalrategst { get; set; }
        public DateTime? RCStart { get; set; }
        public DateTime? RCEndDT { get; set; }
        public int? itemid { get; set; }
    }
}
