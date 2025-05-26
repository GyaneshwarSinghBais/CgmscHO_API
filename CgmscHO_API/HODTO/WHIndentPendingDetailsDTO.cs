using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Net;

namespace CgmscHO_API.Models
{
    public class WHIndentPendingDetailsDTO
    {

        [Key]
        public string? NOCNUMBER { get; set; }


        public string? WAREHOUSENAME { get; set; }
        public string? FACILITYNAME { get; set; }
        public Int64? NOSITEMS { get; set; }
        public string? INDDT { get; set; }
        public string? PENDINGDAY { get; set; }
        public string? PER { get; set; }
        public Double? DMEFAC { get; set; }

        public Int32? AYUSH { get; set; }
        public string? DHS { get; set; }


      



    }
}
