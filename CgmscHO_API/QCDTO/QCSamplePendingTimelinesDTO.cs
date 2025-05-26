using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.QCDTO
{
    public class QCSamplePendingTimelinesDTO
    {
        [Key]
        public string? ID { get; set; }

        public long? mcid { get; set; }

        public string? mcategory { get; set; }

        public long? nositems { get; set; }

        public long? nosbatch { get; set; }

        public double? UQCValuecr { get; set; }

        public string? TimePara { get; set; }

        public string? TimeParaValue { get; set; }

        public string? timeline { get; set; }
    }
}
