
using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.Models
{
    public class StockGroupItemDTO
    {
   
        [Key]
        public Int64 ID { get; set; }
        public string? PARTICULAR { get; set; }
        public Int64? RC { get; set; }
  
        
        public Int64 READYCNT { get; set; }
        public Int64 UQCCOUNT { get; set; }
        public Int64 PIPLINECNT { get; set; }
   
        
    }
}
