
using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.HODTO
{
    public class HODYearWisePODTONew
    {
        [Key]
        public string? ID { get; set; }

        public long? TOTALPOITEMS { get; set; }

        public long? DHSPOITEMS { get; set; }

        public double? DHSPOVALUE { get; set; }

        public double? DHSRECVALUE { get; set; }

        public long? DMEPOITEMS { get; set; }

        public double? DMEPOVALUE { get; set; }

        public double? DMERECVALUE { get; set; }

        public double? TOTALPOVALUE { get; set; }

        public double? TOTALRECVALUE { get; set; }

        public int? ACCYRSETID { get; set; }

        public string? SHACCYEAR { get; set; }
    }
}
