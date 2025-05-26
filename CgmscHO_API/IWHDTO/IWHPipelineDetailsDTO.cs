
using System.ComponentModel.DataAnnotations;
namespace CgmscHO_API.Models
{
    public class IWHPipelineDetailsDTO
    {
      
              [Key]
        public Int64? TRANSFERITEMID { get; set; }

        public string? ITEMCODE { get; set; }
        public string? ITEMNAME { get; set; }

        public string? UNIT { get; set; }
        public string? TRANSFERDATE { get; set; }
        public Int64? TRANSFERQTY { get; set; }
        public string? TRANSFERNO { get; set; }
 
        public string? FROMWAREHOUSENAME { get; set; }
        public Int64? FROMWHCSTOCK { get; set; }
        public Int64? FROMWHUQC { get; set; }

        public string? TOWAREHOUSENAME { get; set; }
        public Int64? TOWHCSTOCK { get; set; }
        public Int64? TOWHUQC { get; set; }
        public string? WHIssUedDT { get; set; }
        public Int64? WHISSUEQTY { get; set; }
        public Int64? PENDINGSINCE { get; set; }

        public Int64? FROMWAREHOUSEID { get; set; }
        public Int64? TOWAREHOUSEID { get; set; }
        public Int64? ITEMID { get; set; }
        public string? MCATEGORY { get; set; }
        public string? TOWHSTOCKOUT { get; set; }
        

    }
}
