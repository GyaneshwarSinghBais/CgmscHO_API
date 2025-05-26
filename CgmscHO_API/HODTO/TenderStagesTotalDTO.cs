using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.HODTO
{
    public class TenderStagesTotalDTO
    {
        [Key]
        public string Status { get; set; }        
        public int NoTenders { get; set; }      
        public int OnOfItems { get; set; }
        public int TenderValue { get; set; }
        
    }
}
