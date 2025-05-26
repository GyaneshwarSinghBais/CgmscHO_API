using System.ComponentModel.DataAnnotations;
namespace CgmscHO_API.Models
{
    public class IWHPipelineSummaryDTO
    {
        [Key]
       
        public Int64? towarehouseid { get; set; }
        public Int64? NOSwhiSSUED { get; set; }
        public Int64? nositems { get; set; }
        public Int64? towhstockout { get; set; }
      
        public Int64? AvgDaysDel { get; set; }
        public string? towarehousename { get; set; }


    }
}
