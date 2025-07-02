namespace CgmscHO_API.HODTO
{
    public class ToBeTenderDetailDTO
    {
        public string? ITEMCODE { get; set; }
        public string? ITEMNAME { get; set; }
        public string? STRENGTH { get; set; }
        public string? UNIT { get; set; }
        public string? EDL { get; set; }

        public decimal? DHSIndnetQty { get; set; }
        public decimal? DHSAIValue { get; set; }
        public decimal? DMEIndentQty { get; set; }
        public decimal? DMEAIValue { get; set; }
        public decimal? TotalIndentQty { get; set; }
        public decimal? TotalAIValue { get; set; }

        public string? SCHEMECODE { get; set; }
        public string? SCHEMENAME { get; set; }
        public string? TENDERREF { get; set; }
        public Int32? lSchemeid { get; set; }
        
    }
}
