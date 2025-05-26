using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.DTO
{
    public class MasRackDTO
    {
        [Key]
        public Int32 RACKID { get; set; }
        public string? LOCATIONNO { get; set; }      

    }
}
