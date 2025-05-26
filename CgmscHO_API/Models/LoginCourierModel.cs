using System.ComponentModel.DataAnnotations.Schema;

namespace CgmscHO_API.Models
{
    [Table("USRUSERS")]
    public class LoginCourierModel
    {
        public Int64 warehouseid { get; set; }
        public string? cpwd { get; set; }      
    }
}
