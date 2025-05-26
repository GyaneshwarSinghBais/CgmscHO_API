using System.ComponentModel.DataAnnotations;
namespace CgmscHO_API.DistrictDTOs
{
    public class DistCGMSCSupplyDTO
    {


        [Key]
        public Int64? DHSAIFixed { get; set; }
        public Int64? IssueaainstIndent { get; set; }
        public Int64? withoutAI { get; set; }
        public Int64? TotalIssuedCGMSCitems { get; set; }
        
       
        public decimal? aginAI_Perc { get; set; }
        
        public decimal? totalPEr { get; set; }
     
    }
}
