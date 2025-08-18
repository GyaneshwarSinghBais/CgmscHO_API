namespace CgmscHO_API.LogAuditDTO
{
    public class getUserPageViewLogsDTO
    {
        public string? logid { get; set; }
        public string? userid { get; set; }
        public string? username { get; set; }
        public string? roleid { get; set; }
        public string? roleidname { get; set; }
        public string? rolecode { get; set; }
        public string? usertype { get; set; }
        public string? pageurl { get; set; }
        public string? pagename { get; set; }
        public string? viewtime { get; set; }
        public string? ipaddress { get; set; }
        public string? useragent { get; set; }
    }
}
