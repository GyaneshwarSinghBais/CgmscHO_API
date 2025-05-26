
using System.ComponentModel.DataAnnotations;
namespace CgmscHO_API.MasterDTO
{
    public class Massupplier
    {
        [Key]

       
        public Int64? supplierid { get; set; }
        public Int64? nos { get; set; }
        public String? suppliername { get; set; }
    }
}
