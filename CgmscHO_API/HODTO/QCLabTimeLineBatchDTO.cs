using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.Models
{
    public class QCLabTimeLineBatchDTO
    {
        

        [Key]
        public string? PHONE1 { get; set; }
        
        public Int64? LABID { get; set; }
        public Int64? MCID { get; set; }
        public string? MCATEGORY { get; set; }
        public string? ITEMCODE { get; set; }
        public string? ITEMNAME { get; set; }
        public Int64? QCDAYSLAB { get; set; }
        public string? ITEMTYPE { get; set; }
        public string? BATCHNO { get; set; }
        public string? LABRECEIPTDATE { get; set; }
        public string? LABNAME { get; set; }
        public Int64? NOSDAYS { get; set; }
        public string? EXCEDDEDSINCETIMELINE { get; set; }


    }
}
