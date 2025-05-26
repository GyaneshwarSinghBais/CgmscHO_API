
using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.Models
{
    public class CGMSCStockDTO
    {
        [Key]
        public Int64 ITEMID { get; set; }
        public string? ITEMCODE { get; set; }
        public string? ITEMNAME { get; set; }
        public string? STRENGTH1 { get; set; }
        public string? SKU { get; set; }

        public string? EDLCAT { get; set; }
        public string? EDL { get; set; }
        
        public Int64 READYFORISSUE { get; set; }
        public Int64 PENDING { get; set; }
        public Int64 TOTLPIPELINE { get; set; }
        
        public string? EDLTYPE { get; set; }
        public string? GROUPNAME { get; set; }
        public string? ITEMTYPENAME { get; set; }
        public string? ISSUEDFY { get; set; }
        

    }
}
