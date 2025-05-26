using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.Models
{
    public class PendingToPickAndDropDTO
    {   
        [Key]
        
        public string? ID { get; set; }
        public string? QDOCKETNO { get; set; }
        public string? ITEMNAME { get; set; }       
        public string? BATCHNO { get; set; }
        public Int64? PENDINGDAYS { get; set; }
        public string? WAREHOUSENAME { get; set; }

    }
}
