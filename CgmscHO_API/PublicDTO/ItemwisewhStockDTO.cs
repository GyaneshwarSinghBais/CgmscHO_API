
using System.ComponentModel.DataAnnotations;
namespace CgmscHO_API.Models
{
    public class ItemwisewhStockDTO
    {
        [Key]
    
        public Int64? ITEMID { get; set; }
     

        public string? ITEMNAME { get; set; }
        public string? ITEMCODE { get; set; }
        public string? ITEMTYPENAME { get; set; }
        public string? GROUPNAME { get; set; }
        public string? SKU { get; set; }
        public string? WAREHOUSENAME { get; set; }

        public string? EDLTYPE { get; set; }

        public Int64? READYFORISSUE { get; set; }
        public Int64? PENDING { get; set; }
        public Int64? ISSUEQTY_CFY { get; set; }

       

    }
}
