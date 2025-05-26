using System.ComponentModel.DataAnnotations;
namespace CgmscHO_API.HODTO
{
    public class DisWiseIssueDTO
    {



        [Key]
        public Int64? DISTRICTID { get; set; }


        public string? DISTRICTNAME { get; set; }
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
