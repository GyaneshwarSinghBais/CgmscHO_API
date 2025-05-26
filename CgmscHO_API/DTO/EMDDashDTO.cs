using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.DTO
{
    public class EMDDashDTO
    {
        [Key]
        public Int32 nossupplierid { get; set; }

        public Int32? nostender { get; set; }
       
        public Double? TotalEMD { get; set; }

      
        public Double? ReleasedEMDAmt { get; set; }

        public Double? PendingEMD { get; set; }


    }
}
