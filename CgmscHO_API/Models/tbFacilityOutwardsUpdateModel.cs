using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CgmscHO_API.Models
{
    [Table("TBFACILITYOUTWARDS")]
    public class tbFacilityOutwardsUpdateModel
    {
        [Key]
        public Int64 FACOUTWARDID { get; set; } = 0;
        public Int64 ISSUEID { get; set; }
        public string STATUS { get; set; }
        public Int64 ISSUED { get; set; }
    }
}
