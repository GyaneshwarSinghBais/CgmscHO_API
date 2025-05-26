using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.Models
{
    public class HODYearWisePODTO
    {
        [Key]
        public string? ID { get; set; }
        
        public Int64? MCID { get; set; }
        public string? MCATEGORY { get; set; }
        public Int64? NOOFITEMS { get; set; }
        public Double? DHSPOVALUE { get; set; }
        public Double? DHSRVALUE { get; set; }

        public Double? DMEPOVALUE { get; set; }
        public Double? DMERVALUE { get; set; }

        public Double? TOTALPOVALUE { get; set; }
        public Double? TOTALRVALUE { get; set; }
       
        public Int32? ACCYRSETID { get; set; }
        public string? ACCYEAR { get; set; }
       
    }
}
