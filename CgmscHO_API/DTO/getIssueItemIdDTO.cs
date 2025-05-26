using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CgmscHO_API.DTO
{
    //[Table("TBFACILITYISSUEITEMS")]
    public class getIssueItemIdDTO
    {
        [Key]
        public Int32 ISSUEITEMID { get; set; }
        public Int32 ISSUEID { get; set; }

        public Int32 ITEMID { get; set; }
    }
}
