using System;

namespace CgmscHO_API.AnalysisDTO
{
    public class PipelineSlippagePOItemDetailDTO
    {
        public string? itemcode { get; set; }
        public string? itemname { get; set; }
        public string? strength1 { get; set; }
        public string? unit { get; set; }
        public string? suppliername { get; set; }
        public string? pono { get; set; }
        public DateTime? soissuedate { get; set; }
        public DateTime? extendeddate { get; set; }
        public decimal? POQTY { get; set; }
        public decimal? ReceivedQTY { get; set; }
        public string? Timduration { get; set; }
    }
}