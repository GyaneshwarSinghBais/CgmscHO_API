using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.DTO
{
    public class getIndentProgramDTO
    {
        [Key]
        public int programid { get; set; }
        public string? program { get; set; }
    }
}
