using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.Models
{
    public class WarehouseWiseReagentDTO
    {
        [Key]    
        public Int64 serial_no { get; set; }
        public string? WAREHOUSENAME { get; set; }
        public string? eqpname { get; set; }
        public Int64? nos { get; set; }
        public Double? stockvalue { get; set; }
        public Int64? warehouseid { get; set; }
        public Int64? mmid { get; set; }
    }
}
