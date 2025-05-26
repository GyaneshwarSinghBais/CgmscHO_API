using System.ComponentModel.DataAnnotations;
namespace CgmscHO_API.FinanceDTO
{
    public class RecFundsDetailsDTO
    {

        [Key]

        public string? RECEIVEDDATE { get; set; }
        public Int64 ACCYRSETID { get; set; }
        public Decimal RecAmt { get; set; }

        public Decimal RecCr { get; set; }

    }
}
