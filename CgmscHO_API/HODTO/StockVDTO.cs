
using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.Models
{
    public class StockVDTO
    {
        [Key]
        public string? ID { get; set; }
        public string? MCID { get; set; }
        public string? EDLCAT { get; set; }
        public string? MCATEGORY { get; set; }
        public Int32? NOOFITEMS { get; set; }
        public Int32? NOOFITEMSREADY { get; set; }
        public Int32? NOOFITEMSUQC { get; set; }

        public Int32? NOOFITEMSPIPELINE { get; set; }
        public Double? READYFORISSUEVALUE { get; set; }
        public Double? QCPENDINGVALUE { get; set; }
        public Double? PIPELINEVALUE { get; set; }
    }
}
