
using System.ComponentModel.DataAnnotations;


namespace CgmscHO_API.Models
{
    public class WHWiseStockDTO
    {
        [Key]
        public string ID { get; set; }
        public string? WAREHOUSENAME { get; set; } 
        public Int64 WAREHOUSEID { get; set; }
        public Int64 READYFORISSUE { get; set; }
        public Int64 PENDING { get; set; }
        public Int64 SUPPLIERPIPELINE { get; set; }
        public Int64 IWHPIPELINE { get; set; }
        public Int64 ISSUEDFY { get; set; }
        



    }
}
