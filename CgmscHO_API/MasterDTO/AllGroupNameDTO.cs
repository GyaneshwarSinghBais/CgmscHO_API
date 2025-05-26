using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.DTO
{
    public class AllGroupNameDTO
    {

        [Key]
        public Int64 GROUPID { get; set; }
        public string? GROUPNAME { get; set; }

    }
}
