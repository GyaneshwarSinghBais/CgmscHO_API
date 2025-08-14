using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.DashHomeDTO
{
    public class RCValidSatusDTO
    {
        [Key]
        public string? edltype { get; set; }
        public Int32? nosIndent { get; set; }
        public Int32? RCValidcnt { get; set; }
        public Int32? RCNotValidcnt { get; set; }
        public Int32? Pricecnt { get; set; }
        public Int32? Evalutioncnt { get; set; }
        public Int32? LiveCnt { get; set; }
        public Int32? Rentendercn { get; set; }
    }
}
