namespace CgmscHO_API.HodDTO
{
    public class variousStatusAgainstCYAIDTO
    {
        public string? itemcode { get; set; }
        public string? itemname { get; set; }
        public string? sku { get; set; }
        public int? unitcount { get; set; }
        public string? edltype { get; set; }
        public decimal? dhsaiqty { get; set; }
        public decimal? dmeaiqty { get; set; }
        public string? RCStatus { get; set; }
        public string? rcenddate { get; set; }
        public decimal? rcrate { get; set; }
        public int? rcremainingdays { get; set; }
        public int? noofsuppliers { get; set; }
        public decimal? dhsissueqty { get; set; }
        public decimal? issueperagdhsai { get; set; }
        public decimal? dmeissueqty { get; set; }
        public decimal? issueperagdmeai { get; set; }
        public decimal? AvgIssueqty_Last3FY { get; set; }
        public decimal? READYSTOCK { get; set; }
        public decimal? UQCSTOCK { get; set; }
        public decimal? PIPELINESTOCK { get; set; }
        public string? tenderstatus { get; set; }
    }
}
