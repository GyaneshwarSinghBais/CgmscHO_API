using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.Models
{
    public class DdlMasreagenteqpDTO
    {
        [Key]
        public int PMACHINEID { get; set; }
        public string? EQPNAME { get; set; }        

    }
}
