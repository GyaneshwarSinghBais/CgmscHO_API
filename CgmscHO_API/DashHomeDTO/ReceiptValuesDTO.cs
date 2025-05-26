using Newtonsoft.Json.Linq;
using System.ComponentModel.DataAnnotations;
namespace CgmscHO_API.DashHomeDTO
{
    public class ReceiptValuesDTO
    {
        [Key]

        public Int64? nosPO { get; set; }
        public Int64? nositems { get; set; }
        public string? RECEIPTDATE { get; set; }
        public string? ReceiptDT { get; set; }

        public Double Rvalue { get; set; }


    }
}
