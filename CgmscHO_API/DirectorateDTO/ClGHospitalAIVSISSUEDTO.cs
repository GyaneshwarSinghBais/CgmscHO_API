using System;
using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.DirectorateDTO
{
    public class ClGHospitalAIVSISSUEDTO
    {
        [Key]
        public long facilityid { get; set; }

        public string? facilityname { get; set; }

        public long nousitemsIndent { get; set; }

        public long Issuenous { get; set; }

        public Decimal? issuedcr { get; set; }
    }
}
