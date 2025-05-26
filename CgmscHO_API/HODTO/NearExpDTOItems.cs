using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.Models
{
    public class NearExpDTOItems
    {

        [Key]
        public Int64? ITEMID { get; set; }
        public string? ITEMCODE { get; set; }
        public string? ITEMNAME { get; set; }
        public Int64? noofbatches { get; set; }
        public Double? NEAREXPVALUE { get; set; }
        public Int64? QTY { get; set; }
        

    }
}


