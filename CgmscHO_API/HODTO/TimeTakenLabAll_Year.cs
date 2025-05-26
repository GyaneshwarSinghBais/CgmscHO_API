using System.ComponentModel.DataAnnotations;
namespace CgmscHO_API.HODTO
{
    public class TimeTakenLabAll_Year
    {
        [Key]
       
        public Int64? LABID { get; set; }
        public String? labname { get; set; }
        public Int64? nossamples { get; set; }




        public Int64? AvgTimeTaken { get; set; }
    }
}
