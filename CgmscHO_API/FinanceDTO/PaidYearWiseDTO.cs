using System.ComponentModel.DataAnnotations;
namespace CgmscHO_API.FinanceDTO
{
    public class PaidYearWiseDTO
    {

        [Key]
        public Int64 ACCYRSETID { get; set; }
        public string? ACCYEAR { get; set; }

        public Int64 NoofPO { get; set; }
        public Decimal AmountPaid { get; set; }


    }
}
