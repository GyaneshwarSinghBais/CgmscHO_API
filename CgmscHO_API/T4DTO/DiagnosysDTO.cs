

using System.ComponentModel.DataAnnotations;
using System.IO;

namespace CgmscHO_API.Models
{
    public class DiagnosysDTO

      
    {
        [Key]
     
         public Int64 ID { get; set; }
        public Int64? FACILITYID { get; set; }
        public string? FACILITYNAME { get; set; }
        public string? PateintType { get; set; }      
        public Int64? NoItemid { get; set; }
        public Int64? PATNO { get; set; }

     


    }
}
