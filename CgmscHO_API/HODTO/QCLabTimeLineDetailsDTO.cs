using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.Models
{
    public class QCLabTimeLineDetailsDTO
    {
        [Key]
        public string? BATCHNO { get; set; }
        
        public Int64? MCID { get; set; }
        public string? MCATEGORY { get; set; }
        public string? ITEMNAME { get; set; }
        public string? ITEMTYPE { get; set; }
        public string? QCDAYSLAB { get; set; }

        public Int64? AVGDAYINLAB { get; set; }
        public Int64? NOSLAB { get; set; }
        
        public Int64? QTY { get; set; }
    
        public string? EXCEDDEDSINCETIMELINE1 { get; set; }
      

    }
}
