using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.DTO
{
    public class SavedFacIndentItemsDTO
    {
        [Key]
        public Int64 SR { get; set; }
        public Int64? ItemID { get; set; }
        public String? itemname { get; set; }
        public Int64? STOCKINHAND { get; set; }
        public Int64? whindentQTY { get; set; }
        public String? status { get; set; }
    }
}
