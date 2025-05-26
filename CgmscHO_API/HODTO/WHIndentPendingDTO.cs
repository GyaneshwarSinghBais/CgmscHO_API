using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Net;

namespace CgmscHO_API.Models
{
    public class WHIndentPendingDTO
    {
 

        [Key]
        public Int64? WAREHOUSEID { get; set; }

        public string? WAREHOUSENAME { get; set; }
        public Int64? NOSINDENT { get; set; }
        public Double? DMEFAC { get; set; }

        public Int32? AYUSH { get; set; }
        public string? DHS { get; set; }
        public string? PER { get; set; }


    }
}
