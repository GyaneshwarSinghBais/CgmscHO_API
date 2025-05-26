using System.ComponentModel.DataAnnotations;
namespace CgmscHO_API.DistrictDTOs;
public class WHStkcount
{
   
    [Key]
    public Int64? WAREHOUSEID { get; set; }
    public string? WAREHOUSENAME { get; set; }
    public Int64? NEDL { get; set; }
    public Int64? EDL { get; set; }
}
