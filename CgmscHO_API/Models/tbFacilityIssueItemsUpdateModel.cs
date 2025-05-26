using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CgmscHO_API.Models
{
    [Table("tbFacilityIssueItems")]
    public class tbFacilityIssueItemsUpdateModel
    {
        [Key]
        public Int64 ISSUEITEMID { get; set; }
        public string STATUS { get; set; }
        public Int64 ISSUED { get; set; }

        //[Table("tbFacilityIssueItems")]
        //public class tbFacilityIssueItemsModel
        //{
        //    [Key]
        //    public Int64 ISSUEITEMID { get; set; }
        //    public string STATUS { get; set; }
        //    public Int64 ISSUED { get; set; }
        //}
    }
}
