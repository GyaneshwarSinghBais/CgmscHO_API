
using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.PublicDTO
{
    public class DeliveryDash
    {

        [Key]
        public Int32? TRAVALEID { get; set; }
        public string? LONGITUDE { get; set; }
        public string? LATITUDE { get; set; }
        public string? DROPDATE { get; set; }
        public string? VEHICALNO { get; set; }
        public string? indDT { get; set; }
        public string? facilityname { get; set; }
        public Int32? nositems { get; set; }
        public Int32? indentid { get; set; }

        
    }
}
