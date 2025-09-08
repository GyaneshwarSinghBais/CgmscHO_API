namespace CgmscHO_API.HODTO
{
    public class ExcessLiftDetailDTO
    {
        public long? facilityid { get; set; }
        public string? facilityname { get; set; }
        public string? itemname { get; set; }
        public string? strength1 { get; set; }
        public string? unit { get; set; }
        public decimal? AI { get; set; }
        public decimal? issueqty { get; set; }
        public decimal? ISSValuers { get; set; }
        public int? Issuenous { get; set; }
        //public long? VI_FACILITYID { get; set; }  // alias for vi.facilityid to avoid duplicate name
        public long? itemid { get; set; }
    }
}
