using System.ComponentModel.DataAnnotations;
namespace CgmscHO_API.FinanceDTO
{
    public class SanctionChequePrepSummaryDTO
    {
    
        [Key]
        public Int64 nosPo { get; set; }
        public Int64 nossupplier { get; set; }

        public Decimal Sncamtcr { get; set; }
        public string? budgetname { get; set; }
        

    }
}
