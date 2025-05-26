using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.Models
{
    public class PendingToReceiptInLabDTO
    {   
        [Key]
        public Int64 ID { get; set; }        
        public Int64? WAREHOUSEID { get; set; }
        public string? WAREHOUSENAME { get; set; }
        public string? QDOCKETNO { get; set; }       
        public Int64? CTID { get; set; }
        public string? ENTRYDATEPICK { get; set; }
        public string? PICKDATE { get; set; }
        public string? DESTINATIONTYPE { get; set; }
        public Int64? DESTINATIONID { get; set; }
        public Int64? WEIGHT { get; set; }
        public string? UNIT { get; set; }
        public string? ITEMNAME { get; set; }
        public string? BATCHNO { get; set; }
        public Int64? ITEMID { get; set; }
        public string? DROPDATE { get; set; }
        public Int64? OUTWNO { get; set; }   
        

    }
}
