using CgmscHO_API.Controllers;
using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.Models
{
    public class LabIssuePendingDetailsDTO
    {   
        [Key]
        public string? ID { get; set; }
        public string? MCATEGORY { get; set; }
        public string? ITEMCODE { get; set; }
        public string? QCDAYSLAB { get; set; }
        public string? UNIT { get; set; }
        
        public string? ITEMNAME { get; set; }       
        public string? BATCHNO { get; set; }
        public Int64? RQTY { get; set; }
        public string? EXPDATE { get; set; }
        public string? WHReceiptDate { get; set; }
        public string? QCReceiptDT { get; set; }
        public string? DELAYPARA { get; set; }
        public string? DELAYPARA1 { get; set; }
        public Int64? MCID { get; set; }
        public Int64? ITEMID { get; set; }
    }
}
