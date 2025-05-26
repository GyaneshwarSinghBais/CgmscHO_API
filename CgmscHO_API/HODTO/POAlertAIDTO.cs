using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.Models
{
    public class POAlertAIDTO
    {
 
        [Key]
        public string? ID { get; set; }
        public Int64? MCID { get; set; }
        public string? RCSTATUS { get; set; }
        public string? EDLTYPE { get; set; }
        public string? TOTAL { get; set; }
        public string? TOBEPOCOUNT { get; set; }
        public string? STOCKOUT { get; set; }
        public string? ONLYQC { get; set; }
        public string? ONLYPIPELINE { get; set; }
       
        public string? EDLTYPEVALUE { get; set; }
        public string? RCSTATUSVALUE { get; set; }
      //  public string? HODID { get; set; }


    }
}
