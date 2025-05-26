
using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.Models
{
    public class dhsDmeYearConsumptionDTO
    {
        [Key]
        public Int64 HODID { get; set; }
        public Int64? ISSUEQTY { get; set; }
        public Int64? UNITCOUNT { get; set; }
        public Int64? ISSUEDSKU { get; set; }     
    }
}
