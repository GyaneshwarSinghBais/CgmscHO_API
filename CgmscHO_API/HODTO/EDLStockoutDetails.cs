using System.ComponentModel.DataAnnotations;
namespace CgmscHO_API.HODTO
{
    public class EDLStockoutDetails
    {



        [Key]

           public Int64? ITEMID { get; set; }
        public String? ITEMNAME { get; set; }
        public String? ITEMCODE { get; set; }
        public String? strength1 { get; set; }
        public String? UNIT { get; set; }
        public String? MCATEGORY { get; set; }

        public Int64? WHAISKU { get; set; }
        public Int64? IssuedQTY { get; set; }
        public Int64? ReadyForIssue { get; set; }
        public Int64? UQC { get; set; }
        public Int64? IWHPipeline { get; set; }
        public Int64? Pipeline { get; set; }

    }
}
