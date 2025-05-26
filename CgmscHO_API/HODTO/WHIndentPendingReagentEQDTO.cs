using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.Models
{
    public class WHIndentPendingReagentEQDTO
    {
        [Key]
        public Int64? MMID { get; set; }
        public string? EQPNAME { get; set; }
        public string? MAKE { get; set; }
        public string? MODEL { get; set; }
        public Int64? NOSFAC { get; set; }
        public Double? INDENTVALUE { get; set; }
        public Int64? NOSWH { get; set; }

    }
}
