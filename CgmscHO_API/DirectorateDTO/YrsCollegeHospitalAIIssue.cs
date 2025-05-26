using System;
using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.DirectorateDTO
{
    public class YrsCollegeHospitalAIIssue
    {
        [Key]
        public long accyrsetid { get; set; }

        public string? ACCYEAR { get; set; }

        public long nousitemsIndent { get; set; }

        public long Issuenous { get; set; }

        public Decimal? issuedcr { get; set; }
    }
}
