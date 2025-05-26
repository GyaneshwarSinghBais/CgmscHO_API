using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CgmscHO_API.Models
{
    [Table("TBFACILITYRECEIPTS")]
    public class tbFacilityReceiptsModel
    {
        [Key]
        public Int64 FACRECEIPTID { get; set; }
        public Int64? FACILITYID { get; set; }

        public Int64? INDENTID { get; set; }
        public Int64? WAREHOUSEID { get; set; }
        public string? FACRECEIPTNO { get; set; }
        public string? FACRECEIPTDATE { get; set; }
        public string? ISUSEAPP { get; set; }
        public string? STATUS { get; set; }
        public string? FACRECEIPTTYPE { get; set; }
        
            
            
    }
}
