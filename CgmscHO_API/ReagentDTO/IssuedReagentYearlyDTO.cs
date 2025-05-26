using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.Models
{
    public class IssuedReagentYearlyDTO
    {
        [Key]    
        
        public int SERIAL_NO { get; set; }
        public string? EQPNAME { get; set; }
        public string? CNTCOUNT { get; set; }
        public string? ISSUEVALUE { get; set; }
        public string? MMID { get; set; }
    }
}
