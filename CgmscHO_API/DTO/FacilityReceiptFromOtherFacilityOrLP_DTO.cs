using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.DTO
{
    public class FacilityReceiptFromOtherFacilityOrLP_DTO
    {
        [Key]
        public Int32 ITEMID { get; set; }
        public Int64? FACINDENTTOWH { get; set; }    
    }
}
