
using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.Models
{
    public class IndentvsIssuanceDTO
    {

       [Key]
        public Int64 MCID { get; set; }
        public Int64 NOS { get; set; }
        public string? MCATEGORY { get; set; }
        public Int64 ISSCOUNT { get; set; }
        public Int64 NOSSTOCK { get; set; }
        public Int64 NOSPIPELINE { get; set; }

        public Int64 RC { get; set; }
        public Int64 ACCEPTED { get; set; }
        public Int64 PRICEOPENED { get; set; }
        public Int64 EVALUATION { get; set; }
        public Int64 LIVE { get; set; }
        public Int64 TOBE { get; set; }
        public Double ISSUEDPER { get; set; }
        

    }
}
