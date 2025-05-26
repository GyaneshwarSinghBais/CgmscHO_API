using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CgmscHO_API.Models
{
    [Table("TBFACILITYISSUES")]
    public class tbFacilityIssuesUpdateModel
    {
        
        [Key]
        public Int64 ISSUEID { get; set; }
        public string STATUS { get; set; }
        public string ISSUEDDATE { get; set; }

        //[Table("TBFACILITYISSUES")]
        //public class tbFacilityIssuesModel
        //{
        //    [Key]            
        //    public Int64 ISSUEID { get; set; }
        //    public string STATUS { get; set; }
        //    public string ISSUEDDATE { get; set; }
        //}
    }
}
