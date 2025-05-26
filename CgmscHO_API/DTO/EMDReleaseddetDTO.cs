using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.DTO
{
    public class EMDReleaseddetDTO
    {
        [Key]
        public Int32 schemeid { get; set; }

        public String? schemename { get; set; }

        public String? suppliername { get; set; }

        public Int64? EMD { get; set; }

      
        public Int64? RealseAmount { get; set; }

        public Int64? PendingEMD { get; set; }
        public String? CHEQUEDT { get; set; }

        public String? Statusdata { get; set; }

        
    }
}
