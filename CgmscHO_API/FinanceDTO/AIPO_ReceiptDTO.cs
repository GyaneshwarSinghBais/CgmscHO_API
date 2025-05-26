using System.ComponentModel.DataAnnotations;
namespace CgmscHO_API.FinanceDTO
{
 
    public class AIPO_ReceiptDTO
    {
        [Key]
        public Int64 AIFINYEAR { get; set; }
        public string? ACCYEAR { get; set; }
        public Int64 NoofItem { get; set; }
        public Int64 NoofPO { get; set; }
        public Decimal POValue { get; set; }

        public Decimal RecValue { get; set; }
        public Decimal totalpaid { get; set; }
        

    }
}
