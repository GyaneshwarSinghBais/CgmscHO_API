using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.DTO
{
    public class FacilityReceiptAgainstIndentDTO
    {
        [Key]
        public Int64 ITEMID { get; set; }
        public Int64? WHISSUETOFAC { get; set; }
        public Int64? FACRECEIPTAGNINDNET { get; set; }
    }
}
