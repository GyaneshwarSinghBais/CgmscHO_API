using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.Models
{
    public class PickRaisedDTO
    {   
        [Key]
        public Int64? INDENTID { get; set; }
        public string WAREHOUSENAME { get; set; }
        public string? QDOCKETNO { get; set; }
        public string? INDENTDATE { get; set; }       
        public string? NOUSITEMS { get; set; }
        public Int64? WAREHOUSEID { get; set; }
       
        public string? INDENTNO { get; set; }   
    }
}
