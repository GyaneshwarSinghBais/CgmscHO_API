using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.WarehouseDTO
{
    public class GetEnterExitVhicleDTO
    {
        [Key]
        public Int64 TRANID { get; set; }
        public string? VPLATENO { get; set; }
        public string? DIRECTION { get; set; }
        public string? VDATE { get; set; } 
        public string? ENTRYDATE { get; set; }
        public Int64? CAMID { get; set; }
        public Int64? WAREHOUSEID { get; set; }
        public string? WAREHOUSENAME { get; set; }
    }
}
