using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CgmscHO_API.Models
{
    [Table("TBFACILITYISSUEITEMS")]
    public class tbFacilityIssueItems
    {
        [Key]       
        public Int32 ISSUEITEMID { get; set; }
        public Int32 ISSUEID { get; set; }
        
        public Int32 ITEMID { get; set; }
        public Int32 CURRENTSTOCK { get; set; }
        public Int32 ALLOTTED { get; set; }
        public Int32 ISSUEQTY { get; set; }
       
        
    }
}
