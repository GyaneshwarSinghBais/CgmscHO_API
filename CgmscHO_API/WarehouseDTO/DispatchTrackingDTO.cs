namespace CgmscHO_API.WarehouseDTO
{
    public class DispatchTrackingDTO
    {
        public int FACILITYID { get; set; }
        public string? FACILITYNAME { get; set; }
        public string? DISTRICTNAME { get; set; }
        public string? ISSUEVOUCHER { get; set; }
        public string? ISSUEVOUCHERDT { get; set; }
        public int NOSITEMS { get; set; }
        public DateTime? TRAVELVOUCHERISSUEDT { get; set; }
        public string? ISPARTIALRELEASE { get; set; }
        public string? INDENTNO { get; set; }
        public string? INDENDT { get; set; }
        public int? VID { get; set; }
        public int? TRAVALEID { get; set; }
        public int? VOUCHERID { get; set; }
        public int? INDENTID { get; set; }
        public string? VEHICALNO { get; set; }
        public int? FACILITYTYPEID { get; set; }
        public int? PARENTFACID { get; set; }
        public string? PARENTFACILITY { get; set; }
        public string? PLONGITUDE { get; set; }
        public string? PLATITUDE { get; set; }
        public DateTime? DROPDATE { get; set; }
        public string? ACKOWLEDGEMOBNO { get; set; }
        public string? ACKOWLEDGEDESIGNATION { get; set; }
        public string? ACKOWLEDGENAME { get; set; }
    }
}
