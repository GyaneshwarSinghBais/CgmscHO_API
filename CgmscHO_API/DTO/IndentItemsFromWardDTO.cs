using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.DTO
{
    public class IndentItemsFromWardDTO
    {
        [Key]
        public Int32 ITEMID { get; set; }
        public string? NAME { get; set; }      

    }
}
