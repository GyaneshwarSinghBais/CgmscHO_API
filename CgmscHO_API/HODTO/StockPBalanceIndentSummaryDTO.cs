using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.HODTO
{
    public class StockPBalanceIndentSummaryDTO
    {
        [Key]
        public Int64? BTYPEORDER { get; set; }
        public String? BTYPE { get; set; }


        public Int64? NOUS { get; set; }
    
    }
}
