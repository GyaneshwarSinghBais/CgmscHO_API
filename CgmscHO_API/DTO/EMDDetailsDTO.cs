using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.DTO
{
    public class EMDDetailsDTO
    {
        [Key]
        public Int32 ID { get; set; }
        public string? CATEGORYNAME { get; set; }
        public string? ACCYEAR { get; set; }
        public string? SCHEMENAME { get; set; }

        public string? STATUSDATA { get; set; }

        public string? SUPPLIERNAME { get; set; }

        public Int64? EMD { get; set; }
        public string? ISRELEASE { get; set; }

        public Int64? REALSEAMOUNT { get; set; }
        public string? RELEASEDATE { get; set; }

        public string? CHEQUENO { get; set; }

        public string? CHEQUEDT { get; set; }

        public string? FILENO { get; set; }


    }
}
