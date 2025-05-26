using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.Models
{
    public class QCSameBatchDetailsDTO
    {
      

        [Key]
        public string? ID { get; set; }
        
        public Int64? ITEMID { get; set; }
  
 
        public string? ITEMCODE { get; set; }
        public string? ITEMNAME { get; set; }
        public string? SUPPLIERNAME { get; set; }
        public string? PONO_QC_DONE { get; set; }
        public string? TESTED_BATCHNO { get; set; }
        public string? TESTED_SAMPLENO { get; set; }
        public string? REPORTNO { get; set; }

        public string? TESTRESULT { get; set; }
        public string? REPORTDATE { get; set; }

        public string? SAMPLENO_PENDING { get; set; }

        public string? SAMPLE_RECEIPTDATE { get; set; }
        public string? PONO_QC_PENDING { get; set; }

    }
}
