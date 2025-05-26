using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.DTO
{
    public class WardIssueItemsDTO
    {
        [Key]
        public Int32 itemid { get; set; }
        public string? name { get; set; }
    }
}
