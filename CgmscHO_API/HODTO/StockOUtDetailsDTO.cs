namespace CgmscHO_API.HODTO
{
    public class StockOUtDetailsDTO
    {
        public string? ITEMCODE { get; set; }
        public string? ITEMNAME { get; set; }
        public string? SKU { get; set; }
        public Int32? UNITCOUNT { get; set; }
        public Int32? DHSAIQTY { get; set; }
        public Int32? DMEAIQTY { get; set; }
        public Int32? AvgIssueqty_Last3FY { get; set; }
        public string? TENDERSTATUS { get; set; }
        public string? TENDERSTARTDT { get; set; }
        public string? COV_A_OPDATE { get; set; }
        public Int32? DAYSSINCE { get; set; }
        public string? ParameterNew { get; set; }
        public decimal? styockPer { get; set; }
        public string? PRICECNT { get; set; }
        public string? EVALUTIONCNT { get; set; }
        public string? LIVECNT { get; set; }
        public string? RENTENDERCN { get; set; }
    }
}
