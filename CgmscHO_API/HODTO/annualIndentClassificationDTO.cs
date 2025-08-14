namespace CgmscHO_API.HODTO
{
    public class annualIndentClassificationDTO
    {
        public String? AIValue { get; set; }
        public int? NosItems { get; set; }
        public decimal? CMEIndentValueCr { get; set; }
        public int? CGMSCStock { get; set; }
        public int? CGMSCStockOut_InPipeline { get; set; }
        public int? NosItemIssuedCFY { get; set; }
        public decimal? IssuedValueCr { get; set; }
        public int? IndentFullfilled { get; set; }
        public int? RC { get; set; }
        public int? UnderTenderEvaluation { get; set; }
        public int? LiveInTender { get; set; }
        public int? TobeRetender { get; set; }
    }
}
