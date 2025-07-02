namespace CgmscHO_API.HODTO
{
    public class SchemeTenderStatusDTO
    {
        public int? SCHEMEID { get; set; }
        public string? SCHEMENAME { get; set; }
        public string? tenderstatus { get; set; }
        public string? tenderremark { get; set; }
        public string? entrydate { get; set; } // Formatted as dd-MM-yyyy in SQL, so use string
        public int? TSID { get; set; }
    }
}
