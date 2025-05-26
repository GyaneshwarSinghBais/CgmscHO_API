using System.ComponentModel.DataAnnotations;
namespace CgmscHO_API.DashHomeDTO
{
    public class DeliveryMonthDTsDTO
    {
     
        [Key]

        public Int64? nooffacIndented { get; set; }
        public Int64? nosindent { get; set; }
        public Int64? IndentIssued { get; set; }
        public Int64? dropindentid { get; set; }
        public Int64? dropfac { get; set; }
    
    }
}
