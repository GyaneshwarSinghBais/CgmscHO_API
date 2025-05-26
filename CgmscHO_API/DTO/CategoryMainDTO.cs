using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.DTO
{
    public class CategoryMainDTO
    {
        [Key]
        public Int32 MCID { get; set; }
        public string? MCATEGORY { get; set; }
    }
}
