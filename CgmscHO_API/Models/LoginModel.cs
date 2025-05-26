using System.ComponentModel.DataAnnotations.Schema;

namespace CgmscHO_API.Models
{
    [Table("USRUSERS")]
    public class LoginModel
    {
        public string emailid { get; set; }
        public string pwd { get; set; }      
    }
}
