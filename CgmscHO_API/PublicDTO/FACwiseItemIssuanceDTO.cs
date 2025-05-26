

using System.ComponentModel.DataAnnotations;
namespace CgmscHO_API.Models
{
    public class FACwiseItemIssuanceDTO
    {
        [Key]

        public Int64 ID { get; set; }
        public Int64 FACILITYID { get; set; }
        public string? FACILITYNAME { get; set; }
        public string? INDENTDATE { get; set; }


        public Int64? ISSUEDSKU { get; set; }
        public Int64? ISSUEDNOS { get; set; }

       

    }
}
