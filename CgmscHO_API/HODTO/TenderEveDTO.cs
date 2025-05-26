using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.Models
{
    public class TenderEveDTO
    {
        [Key]
        public string? CRITERIA { get; set; }
        public string? CRITERIAOR { get; set; }
        
        public Int64? TOTAL { get; set; }
        public Int64? ACCEPT { get; set; }
        public Int64? PRICEBID { get; set; }
        public Int64? COVB { get; set; }

        public Int64? COVAOC { get; set; }
        public Int64? COVA { get; set; }
        public Int64? LIVET { get; set; }
        public Int64? TOBETENDER { get; set; }





    }
}
