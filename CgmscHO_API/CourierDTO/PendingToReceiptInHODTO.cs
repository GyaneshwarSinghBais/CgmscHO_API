using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.Models
{
    public class PendingToReceiptInHODTO
    {   
        [Key]
        public Int64 ID { get; set; }        
        public string? MCID { get; set; }
        public string? MCATEGORY { get; set; }
        public string? ITEMID { get; set; }       
        public string? ITEMCODE { get; set; }
        public string? ITEMNAME { get; set; }
        public string? INDENTID { get; set; }
        public string? INDENTDATE { get; set; }
        public string? QDOCKETNO { get; set; }
        public string? QDOCKETDT { get; set; }
        public string? BATCHNO { get; set; }
        public string? EXPDATE { get; set; }
        public string? ISSUEQTY { get; set; }
        public string? CTID { get; set; }
        public string? OUTWNO { get; set; }
        public string? NOOFDAYS { get; set; }
        public string? PICKDATE { get; set; }
        public string? DROPDATE { get; set; }
        public string? WAREHOUSENAME { get; set; }
        

    }
}
