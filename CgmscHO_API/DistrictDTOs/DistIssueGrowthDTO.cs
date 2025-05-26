
using Newtonsoft.Json.Linq;
using System.ComponentModel.DataAnnotations;
namespace CgmscHO_API.DistrictDTOs
{
    public class DistIssueGrowthDTO
    {
      

        [Key]
        public Int64 ACCYRSETID { get; set; }
        public string? SHACCYEAR { get; set; }
        public Int64 DMEItems { get; set; }
        public Double DMEValue { get; set; }
        public Int64 CMHOItem { get; set; }
        public Double CMHOValue { get; set; }
        public Int64 DHItem { get; set; }
        public Double DHValueLac { get; set; }
        public Int64 CHCitem { get; set; }
        public Double CHCValue { get; set; }
        public Int64 otherfacitems { get; set; }
        public Double Otherfacvalue { get; set; }

        public Int64 AYitems { get; set; }
        public Double AYValue { get; set; }



    }
}
