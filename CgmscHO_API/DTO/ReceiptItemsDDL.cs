using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.DTO
{
    public class ReceiptItemsDDL
    {
        [Key]
        public Int32 INWNO { get; set; }
        public string? NAME { get; set; }      

    }
}
