
using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.HODTO
{
    public class DirectorateIndentPOStatusDTO
    {
  


        [Key]

        public Int64? itemid { get; set; }
        public string? groupname { get; set; }
        public string? itemcode { get; set; }
        public string? itemtypename { get; set; }
        public string? itemname { get; set; }
        
        public string? strength1 { get; set; }
        public string? unit { get; set; }
        public string? edltype { get; set; }
        public Int64? DHSAI { get; set; }
        public Int64? POQTY { get; set; }
        public Double? povalue { get; set; }
        public Int64? RQTY { get; set; }  
        public Double? rvalue { get; set; }
        public string? rPercentage { get; set; }
        public string? edl { get; set; }
        public Int64? groupid { get; set; }
  
    }
}
