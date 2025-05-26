using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.Models
{
    public class HODIssueDTO
    {
        [Key]
        public string? ID { get; set; }
        
        public Int64? MCID { get; set; }
        public string? MCATEGORY { get; set; }
        public Int64? NOOFITEMS { get; set; }
        public Double? ISSUEVALUE { get; set; }
    
        public Int32? ACCYRSETID { get; set; }
        public string? ACCYEAR { get; set; }
       
    }
}
