using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CgmscHO_API.Models
{
    //[Table("MASWAREHOUSES")]
    public class WarehousesModel
    {
        [Key]      
        public Int64 WAREHOUSEID { get; set; }
        public string? WAREHOUSECODE { get; set; }
        public string? WAREHOUSENAME { get; set; }
        public Int64? WAREHOUSETYPEID { get; set; }
        public string? ADDRESS1 { get; set; }
        public string? ADDRESS2 { get; set; }
        public string? ADDRESS3 { get; set; }
        public string? CITY { get; set; }
        public string? ZIP { get; set; }
        public string? PHONE1 { get; set; }
        public string? PHONE2 { get; set; }
        public string? FAX { get; set; }
        public string? EMAIL { get; set; }
        public string? ISACTIVE { get; set; }
        public string? ISGOVERNMENT { get; set; }
        public string? STATEID { get; set; }
        public string? DISTRICTID { get; set; }
        public string? LOCATIONID { get; set; }
        public string? REMARKS { get; set; }
        public string? CONTACTPERSONNAME { get; set; }
        public string? AMID { get; set; }
        public string? SOID { get; set; }
        public string? PARENTID { get; set; }
        public string? GDT_ENTRY_DATE { get; set; }
        public string? FACILITYTYPEID { get; set; }
        public string? FACILITYTYPE { get; set; }
        public string? POPER { get; set; }
        public string? ENTRY_DATE { get; set; }
        public string? ZONEID { get; set; }
        public string? LATITUDE { get; set; }
        public string? LONGITUDE { get; set; }
        public string? ISANPRACTIVE { get; set; }
    }
}
