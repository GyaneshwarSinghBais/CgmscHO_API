namespace CgmscHO_API.HODTO
{
    public class AIissueDetailDTO
    {
        public int? itemid { get; set; }
        public string? itemcode { get; set; }
        public string? itemname { get; set; }
        public string? strength1 { get; set; }
        public string? unit { get; set; }
        public int? unitcount { get; set; }
        public decimal? ai { get; set; }
        public decimal? IssuedQTY { get; set; }
        public decimal? IssuePEr { get; set; }
        public decimal? NocQty { get; set; }
        public int? facilityid { get; set; }
    }
}
