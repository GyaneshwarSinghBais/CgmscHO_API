using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.HODTO
{
    public class ItemIndentQtyDTO

    {
        [Key]
        public String ITEMNAME { get; set; }
        public String? ITEMCODE { get; set; }
        public String? UNIT { get; set; }
        public Int64? DHSAI { get; set; }
        public Int64? DHSISSUE { get; set; }
        public String? DHSISSUEP { get; set; }
        public Int64? DMEAI { get; set; }
        public Int64? DMEISSUE { get; set; }
        public String? DMEISSUEPER { get; set; }
        public Int64? AYUSHAI { get; set; }
        public Int64? AYUSHISSUE { get; set; }
        public String? AYUSHISSUEPER { get; set; }

    }
}
