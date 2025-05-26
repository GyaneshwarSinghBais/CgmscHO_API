using System;
using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.QCDTO
{
    public class QCMonthWisePendingRecDTO
    {
        [Key]
        public long monthid { get; set; }

        public long MCID { get; set; }

        public string? MONTHNAME { get; set; }

        public string? MNAME { get; set; }

        public long nositems { get; set; }

        public Decimal? STKVALUE { get; set; }

        public long? nosbatch { get; set; }

        public string? mcategory { get; set; }
    }
}
