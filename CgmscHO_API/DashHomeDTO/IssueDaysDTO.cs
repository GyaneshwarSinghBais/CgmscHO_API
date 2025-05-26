using System.ComponentModel.DataAnnotations;
namespace CgmscHO_API.DashHomeDTO
{
    public class IssueDaysDTO
    {
        [Key]

        public Int64? nositems { get; set; }
        public string? IndentDT { get; set; }
        public string? indentdate { get; set; }

        public Double TotalValuecr { get; set; }

        public Int64? nosfacility { get; set; }
    }
}
