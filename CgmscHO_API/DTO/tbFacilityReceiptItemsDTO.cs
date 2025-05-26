using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.DTO
{
    public class tbFacilityReceiptItemsDTO
    {
        [Key]
        public Int64 FACRECEIPTITEMID { get; set; }
        public Int64? ABSRQTY { get; set; }       
    }
}
