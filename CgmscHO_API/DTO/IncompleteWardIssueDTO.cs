using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.DTO
{
    public class IncompleteWardIssueDTO
    {
        
        public Int32 WARDID { get; set; }
        public string? WARDNAME { get; set; }
        public string? ISSUENO { get; set; }
        public string? ISSUEDATE { get; set; }
        public string? WREQUESTDATE { get; set; }
        public string? WREQUESTBY { get; set; }
        public string? STATUS { get; set; }
        public Int64? INDENTID { get; set; }
        [Key]
        public int ISSUEID { get; set; }
       
    }
}
