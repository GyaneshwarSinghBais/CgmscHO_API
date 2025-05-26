using System.ComponentModel.DataAnnotations;
namespace CgmscHO_API.DashHomeDTO
{
    public class NearExpMonthHomeDTO
    {


        [Key]
        public string MM { get; set; }

        public string? MONTHNAME { get; set; }
        public string? MNAME { get; set; }
        public Int64? nositems { get; set; }
        public Int64? nosbatches { get; set; }
        public Decimal? STKVALUEcr { get; set; }
      
    }
}
