namespace CgmscHO_API.LogAuditDTO
{
    internal class UserLoginLogs
    {
        public int? UserId { get; set; }
        public int? RoleId { get; set; }
        public string RoleIdName { get; set; }
        public string UserName { get; set; }
        public string IPAddress { get; set; }
        public string UserAgent { get; set; }
    }
}