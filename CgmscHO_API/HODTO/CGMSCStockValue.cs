
using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.Models
{
    public class CGMSCStockValue
    {

        [Key]
        public Int64 ID { get; set; }
        public Int64 MCID { get; set; }
        public string? EDLCAT { get; set; }
        public string? MCATEGORY { get; set; }
        public Int64 NOOFITEMS { get; set; }
        public Int64 NOOFITEMSREADY { get; set; }
        public Int64 NOOFITEMSUQC { get; set; }

        public Int64 NOOFITEMSPIPELINE { get; set; }
        public Double READYFORISSUEVALUE { get; set; }
        public Double QCPENDINGVALUE { get; set; }
        public Double PIPELINEVALUE { get; set; }
           
    }
}
