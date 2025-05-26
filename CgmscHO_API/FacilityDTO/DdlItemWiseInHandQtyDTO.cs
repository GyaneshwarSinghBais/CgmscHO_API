using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.Models
{
    public class DdlItemWiseInHandQtyDTO
    {
        [Key]
        public Int64 ITEMID { get; set; }
        public string? EDLITEMCODE { get; set; }
        public Int64? INHAND_QTY { get; set; }
        public string? DETAIL { get; set; }
        public string? ITEMCODE { get; set; }
        public string? STRENGTH1 { get; set; }
        
    }
}
