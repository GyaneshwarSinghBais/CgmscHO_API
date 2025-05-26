using System.ComponentModel.DataAnnotations;
namespace CgmscHO_API.DashHomeDTO
{
    public class PartiItem_RCDTO
    {
        [Key]
        public Int64 contractitemid { get; set; }
        public Int64 contractid { get; set; }
        public string? schemename { get; set; }
        public string? suppliername { get; set; }
        public string? RANKID { get; set; }
        public string? rcstart { get; set; }
        public string? rcenddt { get; set; }
        public Int64? rcduration { get; set; }
        public Decimal? basicrate { get; set; }
        public Int64? Tax { get; set; }
        public Decimal? RateWithTax { get; set; }
        public string? isextended { get; set; }
        public string? RCStatus { get; set; }
        public string? Remarks { get; set; }

     
    }
}
