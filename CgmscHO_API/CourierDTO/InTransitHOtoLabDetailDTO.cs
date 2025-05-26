using CgmscHO_API.Controllers;
using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.Models
{
    public class InTransitHOtoLabDetailDTO
    {   
        [Key]
        public string? ID { get; set; }
        public string? LABNAME { get; set; }
        public string? MCATEGORY { get; set; }
        public string? ITEMCODE { get; set; }
        public string? QCDAYSLAB { get; set; }
        public string? SAMPLENO { get; set; }
        
        public string? ITEMNAME { get; set; }       
        public string? BATCHNO { get; set; }
        public string? DOCKETNO { get; set; }
  
        public string? LABISSUEDATE { get; set; }
        public string? ENTRYDATEPICK { get; set; }
        public string? DROPDATE { get; set; }
        public string? EXPDATE { get; set; }

        public Int64? RQTY { get; set; }
        public Int64? DAYSSINCEUNDERCOURIER { get; set; }
        
        public string? WHReceiptDate { get; set; }
        public string? QCReceiptDT { get; set; }



        public string? DELAYPARA1 { get; set; }
        public Int64? MCID { get; set; }
        public Int64? ITEMID { get; set; }
        public Int64? LABID { get; set; }
    }
}
