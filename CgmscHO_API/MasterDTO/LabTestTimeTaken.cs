using System.ComponentModel.DataAnnotations;
namespace CgmscHO_API.MasterDTO
{
    public class LabTestTimeTaken
    {
        [Key]

        public Int64? ITEMTYPEID { get; set; }
        public String? ITEMTYPECODE { get; set; }
        public String? ITEMTYPENAME { get; set; }
        
        public Int64? QCDAYSLAB { get; set; }


    }
}
