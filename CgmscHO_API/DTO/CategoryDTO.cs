using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.DTO
{
    public class CategoryDTO
    {
        [Key]
        public Int32 CATEGORYID { get; set; }
        public string? CATEGORYNAME { get; set; }
    }
}
