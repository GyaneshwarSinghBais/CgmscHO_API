namespace CgmscHO_API.HODTO
{
    public class SchemeReceivedDTO
    {
        public int? SCHEMEID { get; set; }
        public string? SCHEMENAME { get; set; }
        public string? FACILITYTYPECODE { get; set; }
        public string? letterno { get; set; }
        public string? letterdate { get; set; } // Formatted as dd-MM-yyyy in SQL, so keep as string
        public string? remarks { get; set; }
        public string? senddate { get; set; } // Formatted as dd-MM-yyyy
        public string? entrydate { get; set; }
        public string? filename { get; set; }
        public string? filepath { get; set; } // ~ replaced with dpdmis.in in SQL
        public int? convid { get; set; }

        public int? CONRID { get; set; }
        public string? RECVDATE { get; set; }
        public string? RECVLETTERNO { get; set; }
        public string? RECVLETTERDT { get; set; }
        public string? RECVREMARK { get; set; }
        public string? RECVFILENAME { get; set; }
        public string? RECVFILEPATH { get; set; } // ~ replaced with dpdmis.in
        public string? RECVENTRYDATE { get; set; }
    }
}
