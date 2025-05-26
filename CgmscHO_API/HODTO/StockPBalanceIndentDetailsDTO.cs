
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;

namespace CgmscHO_API.HODTO
{
    public class StockPBalanceIndentDetailsDTO
    {

        [Key]
        public Int64? ITEMID { get; set; }
        public String? ITEMCODE { get; set; }
        public String? ITEMNAME { get; set; }
        public String? STRENGTH1 { get; set; }
        public String? UNIT { get; set; }
        public String? RCSTATUS { get; set; }
        public Int64? AI { get; set; }
        public Int64? ISSUED { get; set; }
        public Int64? BALANCEINDENT { get; set; }
        public String? ISSUP { get; set; }
        public String? STOCKPER { get; set; }
        
        public Int64? READYSTK { get; set; }
        public Int64? UQCSTK { get; set; }
        public Int64? TOTLPIPELINE { get; set; }
    }
}
