using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.DTO
{
    public class IncompleteWardIndentDTO
    {
        [Key]
        public Int64 NOCID { get; set; }
        public Int64? INDENTID { get; set; }
        public string? WARDNAME { get; set; }
        public string? WREQUESTBY { get; set; }
        public string? WINDENTNO { get; set; }
        public string? WREQUESTDATE { get; set; }
        public string? ISSUEDATE { get; set; }
        public string? ISSUENO { get; set; }
        public string? DSTATUS { get; set; }
        public Int64? NOS { get; set; }

        public Int64? ISSUEID { get; set; }
        public Int64? FACILITYID { get; set; }
        public Int64? WARDID { get; set; }


    }
}
