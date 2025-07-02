using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.HODTO
{
    public class WhMangerSSODetailDTO
    {
        [Key]
        public string? WarehouseId { get; set; }
        public string? WarehouseName { get; set; }
        public string? Address { get; set; }

        public string? AmId { get; set; }
        public string? AmName { get; set; }
        public string? AMMobileNo { get; set; }

        public string? SoId { get; set; }
        public string? SoName { get; set; }
        public string? SOMobileNo { get; set; }
        public string? LATITUDE { get; set; }
        public string? LONGITUDE { get; set; }
        
    }
}
