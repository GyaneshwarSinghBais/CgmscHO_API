using System.ComponentModel.DataAnnotations.Schema;

namespace CgmscHO_API.AnalysisDTO
{
    public class PipelineSlippageDetailDTO
    {
        public string? Timduration { get; set; }
        public decimal? itemid { get; set; }
        public string? itemcode { get; set; }
        public string? itemname { get; set; }
        public decimal? absqty_sum { get; set; }
        public decimal? receiptabsqty_sum { get; set; }
        public decimal? pipelineqty_sum { get; set; }
        public decimal? min_per { get; set; }
        public decimal? worst_d { get; set; }
        public decimal? nospo { get; set; }
    }
}
