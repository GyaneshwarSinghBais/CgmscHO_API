using System.ComponentModel.DataAnnotations;
namespace CgmscHO_API.FinanceDTO
{
    public class PaidDateWiseDTO
    {

        [Key]
        public Int64 ID { get; set; }
        public string? Name { get; set; }

        public Int64 NoofPO { get; set; }
        public Decimal AmountPaid { get; set; }
    }
}
