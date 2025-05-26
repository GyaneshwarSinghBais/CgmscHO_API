using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.DTO
{
    public class IndentDataDTO
    {
        [Key]
        public Int64 NOCID { get; set; }
        public string? REQUESTEDDATE { get; set; }
        public string? WARDID { get; set; }
    }
}
