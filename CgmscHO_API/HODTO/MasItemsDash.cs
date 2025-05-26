

using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.Models
{
    public class MasItemsDash
    {
       
        [Key]
        public Int64 itemid { get; set; }
        public string? NameText { get; set; }
        
        public string? itemname { get; set; }
    
        public Int64? mcid { get; set; }
       
        
    }
}
