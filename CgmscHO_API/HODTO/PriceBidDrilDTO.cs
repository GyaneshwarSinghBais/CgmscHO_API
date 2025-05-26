using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.HODTO
{
    public class PriceBidDrilDTO
    {
        [Key]

        public Int64 ITEMID { get; set; }
        public Int32? SCHEMEID { get; set; }
        public string? ITEMCODE { get; set; }
        public string? ITEMNAME { get; set; }
        public string? SCHEMECODE { get; set; }
        public string? PRICEBIDDATE { get; set; }

      
    }
}
