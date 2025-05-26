using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.DTO
{
    public class getBatchesDTO
    {

 //       select distinct rb.FACReceiptItemid FacReceiptItemID, rb.BatchNo,rb.MfgDate,rb.ExpDate,nvl(rb.AbsRqty,0) AbsRQty,nvl(x.issueQty,0) AllotQty , nvl(rb.AbsRqty,0)-nvl(x.issueQty,0) avlQty
 //, rb.Inwno ,rb.whinwno
 // from

        [Key]
        public Int64 Inwno { get; set; }
        public Int64 FacReceiptItemID { get; set; }
        public string BatchNo { get; set; }

        public string MfgDate { get; set; }
        public string ExpDate { get; set; }
        public Int64 AbsRQty { get; set; }
        public Int64 AllotQty { get; set; }
        public Int64 avlQty { get; set; }
        public Int64 whinwno { get; set; }
    }
}
