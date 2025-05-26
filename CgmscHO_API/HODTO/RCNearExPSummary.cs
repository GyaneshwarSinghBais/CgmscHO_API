using System.ComponentModel.DataAnnotations;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace CgmscHO_API.HODTO
{
    public class RCNearExPSummary
    {
        [Key]
    
    
        public String? MM { get; set; }
        
        public Int64? NOSRC { get; set; }
       
    }
}
