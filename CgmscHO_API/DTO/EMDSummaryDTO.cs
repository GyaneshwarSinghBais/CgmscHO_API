using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.DTO
{
    public class EMDSummaryDTO
    {
        [Key]
        public Int32 supplierid { get; set; }
        public string? suppliername { get; set; }
        public Int32? nostender { get; set; }
        public Int64? TotalEMD { get; set; }

        public Int64? ReleasedEMDAmt { get; set; }

        public Int64? PendingEMD { get; set; }
        
     
    }
}
