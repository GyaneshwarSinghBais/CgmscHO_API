using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CgmscHO_API.Models
{
    //IssueItemID,ItemID,FacReceiptItemID,IssueQty,Issued,Inwno,whinwno
    [Table("TBFACILITYOUTWARDS")]
    public class tbFacilityOutwardsModel
    {
        [Key] 
        public Int64? FACOUTWARDID { get; set; }
        public Int64? ISSUEITEMID { get; set; }

        public Int64? ITEMID { get; set; }

        public Int64? FACRECEIPTITEMID { get; set; }

        public Int64 ISSUEQTY { get; set; }

        public Int64? ISSUED { get; set; }
        public Int64? INWNO { get; set; }
        public Int64? WHINWNO { get; set; }
    }
}
