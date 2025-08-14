using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.HODTO
{
    public class SupplierPendingPaymentsDTO
    {
        [Key]
        public long? SUPPLIERID { get; set; }
        public string? SUPPLIERNAME { get; set; }
        public int? nosPO { get; set; }
        public decimal? RecLibLacs { get; set; }
    }
}
