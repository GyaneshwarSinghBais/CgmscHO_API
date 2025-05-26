using System.ComponentModel.DataAnnotations;
namespace CgmscHO_API.WarehouseDTO
{
    public class DropAppPerfomance
    {

        [Key]
        public Int64 WAREHOUSEID { get; set; }
        public string? WAREHOUSENAME { get; set; }
        public Int64? NOSVEHICLE { get; set; }
        public Int64? NOSDIST { get; set; }
        public Int64? NOSFAC { get; set; }
        public Int64? NOOFFACINDENTED { get; set; }
        public Int64? NOSINDENT { get; set; }
        public Int64? INDENTISSUED { get; set; }
        public Int64? DROPINDENTID { get; set; }
        public Int64? INTRASIT { get; set; }
        public Double? DroPPEr { get; set; }
        public Int64? AVGDAYSTAKENSINCEINDENTREC { get; set; }
       
    }
}
