using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.DTO
{
    public class IncompleteWardIssueItemsDTO
    {
        [Key]
        public Int32 IssueItemID { get; set; }
        public string? ItemCode { get; set; }
        public string? ItemName { get; set; }
        public string? Strength { get; set; }
        public Int32 IssueQty { get; set; }
        public string? batchno { get; set; }
        public string? expdate { get; set; }
        public string? LOCATIONNO { get; set; }      
        public Int32 ItemID { get; set; }
    }
}
