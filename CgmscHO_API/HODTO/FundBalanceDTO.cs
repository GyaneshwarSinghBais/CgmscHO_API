using System.ComponentModel.DataAnnotations;


namespace CgmscHO_API.Models
{
    public class FundBalanceDTO
    {
        [Key]
        public Int64? BUDGETID { get; set; }
        public string? BUDGETNAME { get; set; }
        public Double? OPBALANCE { get; set; }
        public Double? ACTRECYEAR { get; set; }
        public Double? TOTADUJST { get; set; }
        public Double? REFUNDAMT { get; set; }
        public Double? TOTALFUNDAVL { get; set; }
        public Double? TOTPAID { get; set; }
        public Double? CLOSINGBAL { get; set; }

    }
}
