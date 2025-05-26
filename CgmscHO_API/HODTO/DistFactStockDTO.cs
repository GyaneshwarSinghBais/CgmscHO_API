
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.Models
{
    public class DistFactStockDTO
    {
 
        [Key]
        public Int64 FACILITYID { get; set; }        
        public string? FACILITYNAME { get; set; }
        public string? FACNAME { get; set; }
        public Int64? LPSTOCK { get; set; }
        public Int64? STOCK { get; set; }      
        public Int64? TOTALSTOCK { get; set; }

        public Int64? WHISSUED { get; set; }
        public Int64? CONSUMPTION { get; set; }
   

    }
}
