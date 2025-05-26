using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.DTO
{
    public class ItemDetailDTO
    {
        [Key]
        public String ITEMCODE { get; set; }
        public String? ITEMNAME { get; set; }
        public String? STRENGTH1 { get; set; }
        public String? ITEMTYPENAME { get; set; }
        public String? EDLCATNAME { get; set; }
        public String? EDL { get; set; }
        public Int64? AIFACILITY { get; set; }
        public Int64? APPROVEDAICMHO { get; set; }
    }
}
