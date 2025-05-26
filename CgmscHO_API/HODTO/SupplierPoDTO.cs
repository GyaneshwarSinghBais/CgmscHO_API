
using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.Models
{
    public class SupplierPoDTO
    {
        [Key]
        public Int64 SERIAL_NO { get; set; }
        public string? ITEMCODE { get; set; }
        public string? ITEMNAME { get; set; }
        public string? UNIT { get; set; }
        public string? PONO { get; set; }
        public string? PODATE { get; set; }
        public string? POQTY { get; set; }
        public string? LASTMRCDT { get; set; }
        public string? SDDATE { get; set; }
        public string? LASTQCPAADEDDT { get; set; }
        public string? FITDATE { get; set; }
        public string? RVALUELACS { get; set; }
        public string? REASONNAME { get; set; }
        public string? SUPPLIERNAME { get; set; }
        public string? SUPPLIERID { get; set; }
        public string? RECEIPTQTY { get; set; }
        public string? QCPASS { get; set; }
        public string? PRESENTFILE { get; set; }
        public string? FILEFINSTATUS { get; set; }
        public string? FILENO { get; set; }
        public string? FILEDT { get; set; }
        public string? QCTEST { get; set; }
        public string? HODTYPE { get; set; }
        
        public string? FITUNFIT { get; set; }
        public string? BUDGETID { get; set; }
        public string? BUDGETNAME { get; set; }
     

    }
}
