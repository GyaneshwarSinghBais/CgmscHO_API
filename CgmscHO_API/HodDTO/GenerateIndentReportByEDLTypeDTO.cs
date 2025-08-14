namespace CgmscHO_API.HodDTO
{
    public class GenerateIndentReportByEDLTypeDTO
    {
        public string? edltype { get; set; }
        public int? nosIndent { get; set; }
        public decimal? RCValidcnt { get; set; }
        public decimal? RCNotValidcnt { get; set; }
        public decimal? Pricecnt { get; set; }
        public decimal? Evalutioncnt { get; set; }
        public decimal? LiveCnt { get; set; }
        public decimal? Rentendercn { get; set; }
    }
}
