using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CgmscHO_API.Models
{
    [Table("MASCGMSCNOC")]
    public class tbGenIndent
    {
       
        [Key]       
        public Int64 NOCID { get; set; }
        [Required]
        public Int64 PROGRAMID { get; set; }
        public Int64 ACCYRSETID { get; set; }
        public Int64 FACILITYID { get; set; }

        public string? NOCNUMBER { get; set; }
        public string? NOCDATE { get; set; }
        public string? STATUS { get; set; }
        
        public string? AUTO_NCCODE { get; set; }
        
        public string? ISUSEAPP { get; set; }      
        


    }
}
