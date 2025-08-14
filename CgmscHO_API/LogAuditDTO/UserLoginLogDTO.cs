using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;



namespace CgmscHO_API.LogAuditDTO
{
    [Table("USERLOGINLOGS", Schema = "CGMSCL")]
    public class UserLoginLogDTO
    {
        //Properties to hold user login log data
        [Key]
        [Column("LOGID")]
        public long? LogId { get; set; }

        [Column("USERID")]
        public long? UserId { get; set; }

        [Column("ROLEID")]
        public long? RoleId { get; set; }

        [Column("ROLEIDNAME")]
        public string? RoleIdName { get; set; }

        [Column("USERNAME")]
        public string? UserName { get; set; }

        [Column("LOGINTIME")]
        public DateTime? LoginTime { get; set; }


        [Column("IPADDRESS")]
        public string? IPAddress { get; set; }

        [Column("USERAGENT")]
        public string? UserAgent { get; set; }
    }
}
