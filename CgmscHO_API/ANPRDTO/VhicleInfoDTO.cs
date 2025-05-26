using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.ANPRDTO
{
    public class VhicleInfoDTO
    {
        [Key]
        public Int64 TRANID { get; set; }             // Primary key
        public string? VPLATENO { get; set; }
        public string? DIRECTION { get; set; }
        public string? VDATE { get; set; }              // Date as string
        public string? ENTRYDATE { get; set; }          // Date as string
        public Int64? CAMID { get; set; }
        public Int64? WAREHOUSEID { get; set; }
        public string? WAREHOUSENAME { get; set; }
    }
}
