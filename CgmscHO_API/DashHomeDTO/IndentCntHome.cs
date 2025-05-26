using System.ComponentModel.DataAnnotations;
namespace CgmscHO_API.DashHomeDTO
{
    public class IndentCntHome
    {
       
        [Key]
        public string HOD { get; set; }


        public Int64? nositems { get; set; }
        public Int64? Returned { get; set; }
        public Int64? ActualAI { get; set; }

    }
}
