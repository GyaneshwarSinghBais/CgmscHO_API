using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.Models
{
    public class GetItemQtyDTO
    {
        [Key]
        public string ITEMCODE { get; set; }
        public string? ITEMNAME { get; set; }
        public string? DISTRICTNAME { get; set; }        
        public string? STRENGTH { get; set; }
        public string? UNIT { get; set; }      
        public Int64? INHAND_QTY { get; set; }
        
    }
}
