using System.ComponentModel.DataAnnotations;
namespace CgmscHO_API.HODTO
{
    public class TimeTakenYearQC
    {

        [Key]
        public Int64? ACCYRSETID { get; set; }
        public String? ACCYEAR  { get; set; }
        public Int64? POnositems { get; set; }
 

        public Int64? TOTALSAMPLE { get; set; }
  
        public Int64? QCTIMETAKEN { get; set; }
    }
}
