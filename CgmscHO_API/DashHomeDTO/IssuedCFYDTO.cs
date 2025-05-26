using CgmscHO_API.Controllers;
using System.ComponentModel.DataAnnotations;
namespace CgmscHO_API.DashHomeDTO
{
    public class IssuedCFYDTO
    {
        [Key]

        public Int64? mcid { get; set; }
        public string? mcategory { get; set; }
        public Int64? IssueQTY { get; set; }
        
        public Double TotalValuecr { get; set; }

        public Int64? nositems { get; set; }
        public Int64? nosfacility { get; set; }
        
    }
}
