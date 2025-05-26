
using System.ComponentModel.DataAnnotations;
namespace CgmscHO_API.Models
{
    public class DistwiseItemIssuanceDTO
    {
        [Key]

        public Int64? DISTRICTID { get; set; }
     

        public string? DISTRICTNAME { get; set; }
   

        public Int64? ISSUEDSKU { get; set; }
        public Int64? ISSUEDNOS { get; set; }

       

    }
}
