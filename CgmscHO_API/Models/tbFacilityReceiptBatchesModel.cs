using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CgmscHO_API.Models
{
    [Table("TBFACILITYRECEIPTBATCHES")]
    public class tbFacilityReceiptBatchesModel
    {
        [Key]       
        public Int64 INWNO { get; set; }
        [Required]
        public Int64 FACRECEIPTITEMID { get; set; }
        public Int64 ITEMID { get; set; }
        public string ? BATCHNO { get; set; }
        public string? MFGDATE { get; set; }
        public string? EXPDATE { get; set; }
        public Int32? QASTATUS { get; set; }
        public Int64? STOCKLOCATION { get; set; }

        public string? WHISSUEBLOCK { get; set; }

        public Int64? PONOID { get; set; }
        public Int64? WHINWNO { get; set; }

        public Int64? ABSRQTY { get; set; }

        

      
    }
}
