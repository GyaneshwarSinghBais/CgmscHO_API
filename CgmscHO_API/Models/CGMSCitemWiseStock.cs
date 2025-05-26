using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.Models
{
    public class CGMSCitemWiseStock
    {
        [Key]
        public string? ITEMCODE { get; set; }

        public string? ITEMNAME { get; set; }
        public string? STRENGTH1 { get; set; }
        

        public string? SKU { get; set; }

        public string? READYFORISSUE { get; set; }

        public string? PENDING { get; set; }
        public string? WAREHOUSENAME { get; set; }
    }
}
