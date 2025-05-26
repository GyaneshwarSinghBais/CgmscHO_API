
using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.Models
{
    public class SupplierLiabilityDTO
    {
        [Key]
        public Int64 SERIAL_NO { get; set; }
        public string? FITUNFIT { get; set; }
        public string? HODTYPE { get; set; }
        public Int64? NOSPO { get; set; }
        public double? LIBAMT { get; set; }     
    }
}
