using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.Models
{
    public class CurrentLiabilityDTO
    {
        [Key]
        public int TOTALFILE { get; set; }
        public decimal? TOTLIBWITHADM { get; set; }
        public int? NOOFFILEFIT { get; set; }
        public decimal? FITLIBVALEWITHADMIN { get; set; }
        public int? NOOFFILENOTFIT { get; set; }
        public decimal? NOTFITLIBVALEWITHADMIN { get; set; }
        public decimal? PIPELINEVALUE { get; set; }
        public decimal? NOTFITBUTRECEIVED { get; set; }
        public decimal? EXTANDABLE_LIB_CR { get; set; }
        public decimal? SANCAMOUNTADM { get; set; }
        public decimal? TOTALPOVALUE { get; set; }
        public decimal? TOTALPOVALUEADM { get; set; }
        public decimal? RECEIPTVALUE { get; set; }
        public decimal? SANCAMOUNT { get; set; }
        public decimal? WITHELDAMT { get; set; }
        public decimal? FITLIB { get; set; }
        public decimal? NOTFITLIB { get; set; }

    }
}
