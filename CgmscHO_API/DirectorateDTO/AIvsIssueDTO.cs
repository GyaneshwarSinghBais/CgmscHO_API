using System;
using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.DirectorateDTO
{
    public class AIvsIssueDTO
    {
        [Key]
        public long accyrsetid { get; set; }

        public string? ACCYEAR { get; set; }

        public long nosIndent { get; set; }

        public long AIReturn { get; set; }

        public long? issueitems { get; set; }

        public Decimal? IssuedValuecr { get; set; }
    }
}
