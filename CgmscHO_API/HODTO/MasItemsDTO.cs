
using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.Models
{
    public class MasItemsDTO
    {
        [Key]
        public Int64 ITEMID { get; set; }
        public string? NAME { get; set; }
        
        public string? ITEMCODE { get; set; }
        public string? ITEMNAME { get; set; }
        public string? STRENGTH1 { get; set; }
        public string? UNIT { get; set; }
        public Int64? UNITCOUNT { get; set; }
        public Int64? GROUPID { get; set; }
        public string? GROUPNAME { get; set; }
        public Int64? ITEMTYPEID { get; set; }
        public string? ITEMTYPENAME { get; set; }
        public string? EDLCAT { get; set; }
        public string? EDL { get; set; }
        public string? EDLTYPE { get; set; }
        
    }
}
