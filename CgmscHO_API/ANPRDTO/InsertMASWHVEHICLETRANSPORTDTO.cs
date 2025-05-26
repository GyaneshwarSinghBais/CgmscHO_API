using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.ANPRDTO
{
    public class InsertMASWHVEHICLETRANSPORTDTO
    {
        [Key]
        public string plate { get; set; }
        public string date { get; set; }
        public string direction { get; set; }
        public string id { get; set; }
    }
}
