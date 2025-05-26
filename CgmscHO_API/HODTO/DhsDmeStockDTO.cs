
using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.Models
{
    public class DhsDmeStockDTO
    {
        [Key]
        public Int64 ITEMID { get; set; }        
        public string? ITEMNAME { get; set; }
        public string? ITEMCODE { get; set; }
        public Int64? UNITCOUNT { get; set; }
        public Int64? FIELDSTOCK { get; set; }      
        public Int64? FIELDSTOCKSKU { get; set; }
      
    }
}
