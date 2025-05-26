using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.DTO
{
    public class StockReportFacilityDTO
    {
        [Key]
        public int ItemId { get; set; }
        public string? CategoryName { get; set; }
        public string? ItemCode { get; set; }
        public string? itemtypename { get; set; }
        public string? ItemName { get; set; }
        public string? Strength1 { get; set; }
        public decimal? ReadyForIssue { get; set; }
        public int FacilityId { get; set; }
      
        public int CategoryId { get; set; }
        public string? EDLType { get; set; }
        
    }
}
