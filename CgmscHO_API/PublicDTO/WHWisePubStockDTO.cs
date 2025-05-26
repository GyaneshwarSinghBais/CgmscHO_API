
using System.ComponentModel.DataAnnotations;
namespace CgmscHO_API.Models
{
    public class WHWisePubStockDTO
    {
        [Key]

        public Int64? WAREHOUSEID { get; set; }
     

        public string? WAREHOUSENAME { get; set; }
        public string? ITEMCODE { get; set; }
        public string? ITEMNAME { get; set; }
        public string? SKU { get; set; }

        public Int64? READYFORISSUE { get; set; }
        public Int64? PENDING { get; set; }
        public Int64? ISSUEQTY_CFY { get; set; }

       

    }
}
