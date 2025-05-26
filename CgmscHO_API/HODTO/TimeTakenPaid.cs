using System.ComponentModel.DataAnnotations;
namespace CgmscHO_API.HODTO
{
    public class TimeTakenPaid
    {
        [Key]
      
        public Int64? YRID { get; set; }
        public String? YR { get; set; }

        public Int64? NOSPO { get; set; }
        public Double? Gross { get; set; }
        public Int64? avgdayssincerec { get; set; }
        public Int64? avgdayssinceQC { get; set; }
    }
}
