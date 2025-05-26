using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.HODTO
{
    public class MasWHInfoDTO
    {
        [Key]
        public Int64 WAREHOUSEID { get; set; }
        public string? WAREHOUSENAME { get; set; }

        public string? AMNAME { get; set; }
        public string? MOB1 { get; set; }
        public string? amid { get; set; }
        public string? ADDRESS1 { get; set; }
        public string? ADDRESS2 { get; set; }
        public string? ADDRESS3 { get; set; }
        public string? ZIP { get; set; }
        public string? EMAIL { get; set; }
    }
}
