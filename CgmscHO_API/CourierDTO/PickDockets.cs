using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.Models
{
    public class PickDockets
    {
   
        [Key]       
        public Int64 indentid { get; set; }
        public string? QDOCKETNO { get; set; }
        public string? warehousename { get; set; }
        public Int64? warehouseid { get; set; }
        public string? indentno { get; set; }
        public string? indentdate { get; set; }
        public Int64? nousitems { get; set; }
        public string? details { get; set; }
        
    }
}
