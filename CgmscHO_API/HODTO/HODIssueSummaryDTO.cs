using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.Models
{
    public class HODIssueSummaryDTO
    {
      
       
        public string? ID { get; set; }
        
        public Int64? mcid { get; set; }
        public string? mcategory { get; set; }
        public Int64? TOTALISSUEITEMS { get; set; }
        public Int64? DHSISSUEITEMS { get; set; }
        public Int64? DMEISSUEITEMS { get; set; }
        public Int64? AYIssueitems { get; set; }

        public Double? TOTALISSUEVALUE { get; set; }
        public Double? DHSISSUEVALUE { get; set; }
        public Double? DMEISSUEVALUE { get; set; }
        public Double? AYISSUEVAL { get; set; }

        [Key]
        public Int32? ACCYRSETID { get; set; }
        public string? SHACCYEAR { get; set; }
       
    }
}
