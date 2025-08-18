namespace CgmscHO_API.LogAuditDTO
{
    public class getUserLoginLogsDTO
    {
        public decimal? logid { get; set; }
        public decimal? userid { get; set; }
        public string? username { get; set; }
        public decimal? roleid { get; set; }
        public string? roleidname { get; set; }
        public string? rolecode { get; set; }
        public string? usertype { get; set; }
        public DateTime? logintime { get; set; }
        public string? ipaddress { get; set; }
        public string? useragent { get; set; }
    }
}
