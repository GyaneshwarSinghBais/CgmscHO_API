namespace CgmscHO_API.QCDTO
{
    public class NsqDrugDetailsDTO
    {
        public string? ItemCode { get; set; }
        public string? ItemName { get; set; }
        public string? BatchNo { get; set; }
        public DateTime? MfgDate { get; set; }
        public DateTime? ExpDate { get; set; }
        public decimal? Stock { get; set; }
        public decimal? FinalRate { get; set; }
        public decimal? StkValue { get; set; }
    }
}
