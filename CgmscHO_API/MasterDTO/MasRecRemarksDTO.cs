using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.Models
{
    public class MasRecRemarksDTO
    {
       
        [Key]
        public Int64? REMID { get; set; }
        public string? REMARKS { get; set; }
   
        


    }
}
