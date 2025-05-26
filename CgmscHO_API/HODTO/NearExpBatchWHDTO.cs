using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.Models
{
    public class NearExpBatchWHDTO
    {
 
        [Key]
        public Int64? ITEMID { get; set; }
        public string? ITEMCODE { get; set; }
        public string? ITEMNAME { get; set; }
        public string? WH { get; set; }
        public string? BATCHNO { get; set; }
        public string? EXPDATE { get; set; }

        public Double? NEAREXPVALUE { get; set; }
        public Int64? QTY { get; set; }
        

    }
}


