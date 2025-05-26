using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.Models
{
    public class UnpaidSupplierDTO
    {
        [Key]
        public Int32 SUPPLIERID { get; set; }
        public string? NOSPO { get; set; }      

    }
}
