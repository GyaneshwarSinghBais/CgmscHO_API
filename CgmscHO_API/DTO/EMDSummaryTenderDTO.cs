using CgmscHO_API.Controllers;
using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.DTO
{
    public class EMDSummaryTenderDTO
    {
        [Key]
        public Int32 schemeid { get; set; }
      
        public string? SCHEMENAME { get; set; }

        public string? STATUSDATA { get; set; }


        public Int32? NOSSUPPLIER { get; set; }
        public Int64? TotalEMD { get; set; }
        public Int64? ReleasedEMDAmt { get; set; }

        public Int64? PendingEMD { get; set; }


    }
}
