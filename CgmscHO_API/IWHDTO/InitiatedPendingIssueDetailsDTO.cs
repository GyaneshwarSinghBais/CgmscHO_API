
using System.ComponentModel.DataAnnotations;
namespace CgmscHO_API.Models
{
    public class InitiatedPendingIssueDetailsDTO
    {
        [Key]
        public Int64? TRANSFERITEMID { get; set; }
        public string? ITEMCODE { get; set; }
        public string? ITEMNAME { get; set; }

        public string? UNIT { get; set; }
        public string? INITIATEDDT { get; set; }
        public Int64? transferqty { get; set; }
        public string? transferno { get; set; }
        public string? FROMWAREHOUSENAME { get; set; }
        public Int64? fromwhCstock { get; set; }
        public Int64? fromWHUQC { get; set; }

        public string? towarehousename { get; set; }
        public Int64? towhCstock { get; set; }
        public Int64? toWHUQC { get; set; }
        public Int64? pendingsince { get; set; }

        public Int64? FROMWAREHOUSEID { get; set; }
        public Int64? towarehouseid { get; set; }
        public Int64? itemid { get; set; }
        public Int64? categoryid { get; set; }
        public string? TOWHSTOCKOUT { get; set; }
        

    }
}
