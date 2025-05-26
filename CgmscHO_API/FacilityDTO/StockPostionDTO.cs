using System.ComponentModel.DataAnnotations;
namespace CgmscHO_API.Models
{
    public class StockPostionDTO
    {
        [Key]
   
        public Int64? FACILITYID { get; set; }
     

        public string? FACILITYNAME { get; set; }
        public Int64? EDLSTOCK { get; set; }
        public Int64? NEDLSTOCK { get; set; }
        public Int64? TOTALSTOCK { get; set; }

       

    }
}
