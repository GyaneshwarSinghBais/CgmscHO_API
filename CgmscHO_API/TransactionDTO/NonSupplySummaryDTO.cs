using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.TransactionDTO
{
    public class NonSupplySummaryDTO
    {
        [Key]
        public Int32? SupplierId { get; set; }
        public string? SupplierName { get; set; }
        public Int32? Nos { get; set; }
    }
}
