using System;
using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.QCDTO
{
    public class QCHomeDashDTO
    {

        [Key]
        public long MCID { get; set; }

        public long nositems { get; set; }

        public Decimal? STKVALUE { get; set; }

        public long? nosbatch { get; set; }

        public string? mcategory { get; set; }

    }
}
