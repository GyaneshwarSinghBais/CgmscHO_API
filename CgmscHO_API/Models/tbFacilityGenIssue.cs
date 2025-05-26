using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CgmscHO_API.Models
{
    [Table("TBFACILITYISSUES")]
    public class tbFacilityGenIssue
    {
        [Key]       
        public Int64 ISSUEID { get; set; }
        [Required]
        public Int64 FACILITYID { get; set; }
     
        public string? ISSUENO { get; set; }
        public string? ISSUEDATE { get; set; }
        public string? ISSUEDDATE { get; set; }
        
        public string? WREQUESTDATE { get; set; }

        public string? WREQUESTBY { get; set; }
        public string? ISUSEAPP { get; set; }

        public string? ISSUETYPE { get; set; }


        public Int64 WARDID { get; set; }
        public Int64? INDENTID { get; set; }
        


    }
}
