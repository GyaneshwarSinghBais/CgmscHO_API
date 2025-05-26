using System;
using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.DirectorateDTO
{
    public class ClgHospitalWithoutAIIssueDTO
    {
        [Key]
        public long facilityid { get; set; }

        public string? facilityname { get; set; }

        public long NosWithAIItems { get; set; }

        public Decimal? issuedcr { get; set; }
    }
}
