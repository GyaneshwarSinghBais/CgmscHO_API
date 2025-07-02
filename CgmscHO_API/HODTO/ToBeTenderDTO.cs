using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.HODTO
{
    public class ToBeTenderDTO
    {
        [Key]
        public Int32? cntItems { get; set; }
        public decimal? AIValue { get; set; }
    }
}
