using System.ComponentModel.DataAnnotations;
namespace CgmscHO_API.FinanceDTO
{
    public class YrLibDTO
    {

        [Key]
        public Int64? ID { get; set; }
        public string? Name { get; set; }
        public Int64? NOSPO { get; set; }

        public Decimal? POVALUE { get; set; }
        public Decimal? RECEIVEDVALUE { get; set; }
        public Decimal? POPIPELINEVALUE { get; set; }
        public Decimal? TOTPAID { get; set; }
        public Decimal? LIBILITY { get; set; }

     

    }
}
