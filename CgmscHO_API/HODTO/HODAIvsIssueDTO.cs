using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.Models
{
    public class HODAIvsIssueDTO
    {



        [Key]
        public string? ID { get; set; }
        
        public Int64? NOSAI { get; set; }
        public Int64? AIRETURED { get; set; }
        public Int64? ACTUALAI { get; set; }
        public Double? AIVALUE { get; set; }
        public Int64? ISSUEDCOUNT { get; set; }
        public Double? ISSUEDVALUE { get; set; }
        public string? MCATEGORY { get; set; }
        
       
    }
}
