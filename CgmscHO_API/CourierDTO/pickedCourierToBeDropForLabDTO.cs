using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.Models
{
    public class pickedCourierToBeDropForLabDTO
    {
   
        [Key]      
       
        public Int64? ID { get; set; }
        public string? SOURCEID { get; set; }
        public Int64? CTID { get; set; }
       
        public string? DOCKET { get; set; }
        public string? DOCKETNO { get; set; }
        public string? DOCKETDATE { get; set; }
        public string? LABID { get; set; }
        public string? LABNAME { get; set; }
        public string? ADDRESS1 { get; set; }
        public string? ADDRESS2 { get; set; }
        public string? ADDRESS3 { get; set; }
        public string? CITY { get; set; }
        public string? ZIP { get; set; }
        public string? PHONE1 { get; set; }
        public string? CNTITEMS { get; set; }
        public string? WEIGHT { get; set; }
        

    }
}
