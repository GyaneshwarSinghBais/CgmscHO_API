using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.QCDTO
{
    public class LabWisePendingDTO
    {
        [Key]
        public string? labid { get; set; }

        public string? labname { get; set; }

        public long? WithBatches { get; set; }

        public long? outBatches { get; set; }

        public double? Withvalue { get; set; }

        public double? outvalue { get; set; }

        public double? outPer { get; set; }
    }
}
