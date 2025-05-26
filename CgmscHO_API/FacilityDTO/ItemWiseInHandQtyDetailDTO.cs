using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.Models
{
    public class ItemWiseInHandQtyDetailDTO
    {
        [Key]
        public Int64 ID { get; set; }
        public Int64 ITEMID { get; set; }
        public string? FACILITYNAME { get; set; }
        public Int64? INHAND_QTY { get; set; }
        public string? FACILITYID { get; set; }
        
    }
}
