using System.ComponentModel.DataAnnotations;
namespace CgmscHO_API.Models
{
    public class StockFACDTO
    {
        [Key]

        public Int64? FACILITYID { get; set; }
        public Int64? FACILITYTYPEID { get; set; }

        public string? FACILITYNAME { get; set; }
        public Int64? NOSITEMS { get; set; }
        public Int64? FACSTKCNT { get; set; }
        public Int64? STOCKOUTNOS { get; set; }

        public Int64? STOCKOUTP { get; set; }
        public Int64? WHSTKCNT { get; set; }
        public Int64? CMHOSTKCNT { get; set; }
        public Int64? WHUQCStkcnt { get; set; }
        public Int64? INDENT_TOWH_PENDING { get; set; }
        public Int64? WHISSUE_REC_PENDING_L180CNT { get; set; }
        public Int64? BALIFT6MONTH { get; set; }
        public Int64? LP_PIPELINE180CNT { get; set; }
        public Int64? NOCTAKEN_NO_LPO { get; set; }

    }
}
