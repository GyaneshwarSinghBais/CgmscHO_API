using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.DashHomeDTO
{
    public class GetWHStockItemsDTO
    {
        [Key]
        public Int64 ITEMID { get; set; }
        public String? itemname { get; set; }
        public String? itemcode { get; set; }
        public String? itemtypename { get; set; }
        public String? unitcount { get; set; }
        public String? strength1 { get; set; }

        public Int64 ReadySTK { get; set; }
        public Int64 UqcSTK { get; set; }
    }
}
