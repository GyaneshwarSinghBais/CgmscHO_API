using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.HODTO
{
    public class RCNearExDetails
    {
        [Key]
     
        
        public Int64? itemid { get; set; }
        public String? itemcode { get; set; }
        public String? itemname { get; set; }
        public String? strength1 { get; set; }
        public String? unit { get; set; }
        public Double? basicrate { get; set; }
        public Int64? GST { get; set; }
        public Double? finalrategst { get; set; }
        public String? RCStart { get; set; }
        public String? RCEndDT { get; set; }
    }
}
