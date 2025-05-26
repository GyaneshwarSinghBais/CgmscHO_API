using System.ComponentModel.DataAnnotations;
namespace CgmscHO_API.HODTO
{
    public class MonthwiseIssueDTO
    {

        [Key]
        public Int64? MON { get; set; }

        public Int64? ID { get; set; }
        public string? MonthName { get; set; }
        public Int64? TOTALISSUEITEMS { get; set; }
        public Int64? DHSISSUEITEMS { get; set; }
        public Int64? DMEISSUEITEMS { get; set; }
        public Int64? AYIssueitems { get; set; }

        public Double? TOTALISSUEVALUE { get; set; }
        public Double? DHSISSUEVALUE { get; set; }
        public Double? DMEISSUEVALUE { get; set; }
        public Double? AYISSUEVAL { get; set; }
    }
}
