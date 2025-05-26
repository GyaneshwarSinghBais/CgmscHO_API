
using System.ComponentModel.DataAnnotations;


namespace CgmscHO_API.Models
{
    public class FundsDTO
    {
        [Key]
        public Int64 BUDGETID { get; set; }
        public string? BUDGETNAME { get; set; } 

    }
}
