namespace CgmscHO_API.HODTO
{
    public class StatusItemDetailDTO
    {
        public int? Tid { get; set; }                   // 11880 → Integer
        public string? ItemCode { get; set; }           // D280 → String
        public string? ItemName { get; set; }           // Iodine Solution IP → String
        public string? Strength { get; set; }           // 8mg / 5ml → String
        public string? Unit { get; set; }               // 60 ml Bottle → String
        public string? IsEdl2021 { get; set; }          // Non EDL → String
        public string? PriceFlag { get; set; }          // N → String
        public int? ToNoOfParticipant { get; set; }     // 0 → Integer
        public decimal? L1Basic { get; set; }           // Price like 12.34 → Decimal (nullable)
        public decimal? IndValue { get; set; }
        
    }
}
