using CgmscHO_API.Controllers;
using System.ComponentModel.DataAnnotations;
namespace CgmscHO_API.DashHomeDTO
{
    public class CGMSCIndentPendingDTO
    {
        [Key]

        public Int64? nosfac { get; set; }
        public Int64? nosIndent { get; set; }

        public Int64? nositems { get; set; }
    }
}
