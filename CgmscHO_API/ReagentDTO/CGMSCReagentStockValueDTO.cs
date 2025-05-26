using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.Models
{
    public class CGMSCReagentStockValueDTO
    {
        [Key]  
        public int SERIAL_NO { get; set; }
        public string? EQPNAME { get; set; }
        public Int64? REQUIRED { get; set; }
        public Int64? NOS { get; set; }
        public double? STOCKVALUE { get; set; }
        public Int64? MMID { get; set; }
        public Int64? NOSWH { get; set; }
        
    }
}
