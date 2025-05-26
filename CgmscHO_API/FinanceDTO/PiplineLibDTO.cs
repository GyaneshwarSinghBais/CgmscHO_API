using System.ComponentModel.DataAnnotations;
namespace CgmscHO_API.FinanceDTO
{
    public class PiplineLibDTO
    {
     
        [Key]
        public Int64 ID { get; set; }
        public string? Name { get; set; }
        public Int64 nositems { get; set; }
        public Int64 nospo { get; set; }
        public Decimal PipeLIvalue { get; set; }


    }
}
