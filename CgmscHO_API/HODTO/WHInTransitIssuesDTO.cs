
using System.ComponentModel.DataAnnotations;


namespace CgmscHO_API.Models
{
    public class WHInTransitIssuesDTO
    {
 
       [Key]
        public Int64 PROGRESSID { get; set; }
        public string? PROGRESS { get; set; }
        public string? REMARKS { get; set; }
        public string? ENTRYDATE { get; set; }
        public string? WAREHOUSENAME { get; set; }
        
        public string? SUPPLIERNAME { get; set; }
        public string? PONO { get; set; }
        public string? PODATE { get; set; }
        public string? ITEMCODE { get; set; }
        public string? ITEMNAME { get; set; }
        public string? REMID { get; set; }
        public string? HOREMARKS { get; set; }
        public string? ENTRYDT { get; set; }
       





    }
}
