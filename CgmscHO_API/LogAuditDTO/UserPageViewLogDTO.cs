using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CgmscHO_API.LogAuditDTO
{
    [Table("USERPAGEVIEWLOGS", Schema = "CGMSCL")]
    public class UserPageViewLogDTO
    {
        // Properties to hold user page view log data
        [Key]
        [Column("LOGID")]
        public long? LogId { get; set; } // LogId is auto-generated, so it can be nullable
        [Column("USERID")]
        public long? UserId { get; set; }
        [Column("ROLEID")]
        public long? RoleId { get; set; }
        [Column("ROLEIDNAME")]
        public string? RoleIdName { get; set; }
        [Column("PAGEURL")]
        public string? PageUrl { get; set; }
        [Column("PAGENAME")]
        public string? PageName { get; set; }
        [Column("VIEWTIME")]
        public DateTime? ViewTime { get; set; } // Default value can be handled in the database
        [Column("IPADDRESS")]
        public string? IPAddress { get; set; }
        [Column("USERAGENT")]
        public string? UserAgent { get; set; }
    }
}