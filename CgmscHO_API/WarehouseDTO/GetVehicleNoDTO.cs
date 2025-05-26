using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.WarehouseDTO
{
    public class GetVehicleNoDTO
    {
        [Key ]
        public Int32 VID { get; set; }
        public string? VEHICALNO { get; set; }
    }
}
