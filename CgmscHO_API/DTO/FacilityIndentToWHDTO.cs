using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.DTO
{
    public class FacilityIndentToWHDTO
    {
        [Key]
        public Int32 ITEMID { get; set; }
        public Int32? FACINDENTTOWH { get; set; }      
    }
}
