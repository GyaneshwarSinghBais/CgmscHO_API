using System.ComponentModel.DataAnnotations;
namespace CgmscHO_API.HODTO
{
    public class EDLstockout
    {
     
        [Key]
        public Int64 WAREHOUSEID { get; set; }
        public string? warehousename { get; set; }
        public Int64 nosdrugs { get; set; }
        public Int64 stockout { get; set; }
        public Double stockoutP { get; set; }
        
       
    }
}
