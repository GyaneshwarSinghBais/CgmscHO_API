namespace CgmscHO_API.TransactionDTO
{
    public class DmeFacNocDetailDTO
    {
        public int? mcid { get; set; }
        public string? mcategory { get; set; }
        public string? EDlType { get; set; }
        public string? facilityname { get; set; }
        public string? districtname { get; set; }
        public long? itemid { get; set; }
        public string? itemcode { get; set; }
        public string? itemname { get; set; }
        public string? strength1 { get; set; }
        public string? unit { get; set; }
        public string? unitc { get; set; }

        public decimal? FacAIQty { get; set; }
        public decimal? CGMSCissueqty { get; set; }
        public int? cntNoc { get; set; }
        public decimal? NocQty { get; set; }
        public decimal? NOCValue { get; set; }
        public decimal? POSKU { get; set; }
        public decimal? povalue { get; set; }
        public decimal? ReceiptqtySKU { get; set; }
        public decimal? recvalue { get; set; }

        public long? facilityid { get; set; }
    }
}
