using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.DTO
{
    public class HoldItemStockDTO
    {
       
        public string? ITEMCODE { get; set; }
        public string? ITEMNAME { get; set; }
        public string? strength1 { get; set; }
        public string? batchno { get; set; }
        public string? expdate { get; set; }
        public string? whissueblock { get; set; }
        
        public decimal? HoldStock { get; set; }

        [Key]
        public int inwno { get; set; }
       
    }
}
