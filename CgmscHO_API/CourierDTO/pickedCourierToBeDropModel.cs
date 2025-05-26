using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.Models
{
    public class pickedCourierToBeDropModel
    {
   
        [Key]       
       // public Int64 INDENTID { get; set; }
        public string? QDOCKETNO { get; set; }
        public string? WAREHOUSENAME { get; set; }
        public Int64? WAREHOUSEID { get; set; }
        //public string? INDENTNO { get; set; }
        //public string? INDENTDATE { get; set; }
        //public Int64? NOUSITEMS { get; set; }
        public string? DETAILS { get; set; }
        public string? ENTRYDATEPICK { get; set; }
        public string? PICKDATE { get; set; }
        public string? DESTINATIONTYPE { get; set; }
        public string? DESTINATIONID { get; set; }
        public string? WEIGHT { get; set; }
        public string? UNIT { get; set; }
        public string? CTID { get; set; }
        public string? STATUS { get; set; }
        public string? ENTRYDATE { get; set; } //status history ctid

    }
}
