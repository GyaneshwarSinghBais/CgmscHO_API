using System;
using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.DirectorateDTO
{
    public class DirectorateWithoutAIDTO
    {
        [Key]
        public long IssueYearID { get; set; }

        public string? ACCYEAR { get; set; }

        public long nositemsissued { get; set; }

        public Decimal? IssuedValuecr { get; set; }
    }
}
