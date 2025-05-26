using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.DTO
{
    public class GetAyushItemsDTO
    {
        [Key]
        public Int64 ITEMID { get; set; }
        public String? ITEMNAMEPAR { get; set; }
        public String? ITEMNAME { get; set; }
    }
}
