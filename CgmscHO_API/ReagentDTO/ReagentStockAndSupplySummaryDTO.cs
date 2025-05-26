using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.Models
{
    public class ReagentStockAndSupplySummaryDTO
    {
        [Key]  
        public string STKP { get; set; }
        public string? NOS { get; set; }     
        
    }
}
