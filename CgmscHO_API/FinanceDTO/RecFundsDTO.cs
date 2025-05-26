using System.ComponentModel.DataAnnotations;
namespace CgmscHO_API.FinanceDTO
{
    public class RecFundsDTO
    {
  
        [Key]
        public Int64 ACCYRSETID { get; set; }
        public string? ACCYEAR { get; set; }
        public Decimal RecAmt { get; set; }
 
        public Decimal refund { get; set; }
        public Decimal adjust { get; set; }
  
    }
}
