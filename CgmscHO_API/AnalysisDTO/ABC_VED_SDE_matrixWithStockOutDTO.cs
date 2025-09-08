namespace CgmscHO_API.AnalysisDTO
{
    public class ABC_VED_SDE_matrixWithStockOutDTO
    {
        public string? category { get; set; }
        public Int32? cntItems { get; set; }
        public Int32? STOCKOUT { get; set; }
        public Int32? STOCKIN { get; set; }
        public Int32? STOCKOUTPOPIPE { get; set; }
        public Int32? RCVALID { get; set; }
        public Int32? RCNOTVALID { get; set; }
        public Int32? PRICECNT { get; set; }
        public Int32? EVALUTIONCNT { get; set; }
        public Int32? LIVECNT { get; set; }
        public Int32? RENTENDERCN { get; set; }
    }
}
