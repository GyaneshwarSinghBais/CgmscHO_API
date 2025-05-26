using System.ComponentModel.DataAnnotations;
namespace CgmscHO_API.HODTO
{
    public class SupplierTimeTaken
    {

        [Key]
        public string? ID { get; set; }
        public Int64? ACCYRSETID { get; set; }
        public string? ACCYEAR { get; set; }

        public Int64? nospo { get; set; }
        public Int64? nositems { get; set; }
        public Int64? TimetakenSupply { get; set; }
        
      
    }
}
