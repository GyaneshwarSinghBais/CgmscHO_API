using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CgmscHO_API.Models
{
    [Table("PRODUCTS")]
    public class ProductsInfo
    {
        [Key]
        public int PRODUCTRECORDID { get; set; }
        [Required]
        [StringLength(100)]
        public string PRODUCTID { get; set; } = String.Empty;
        [StringLength(400)]
        public string PRODUCTNAME { get; set; } = String.Empty;
        [StringLength(200)]
        public string MANUFACTURER { get; set; } = String.Empty;
        [StringLength(1000)]
        public string DESCRIPTION { get; set; } = String.Empty;
        public int PRICE { get; set; }
    }
}
