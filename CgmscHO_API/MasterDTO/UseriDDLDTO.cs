using System.ComponentModel.DataAnnotations;
namespace CgmscHO_API.MasterDTO
{
    public class UseriDDLDTO
    {
    
        [Key]
        public Int64 userid { get; set; }
        public string? textfield { get; set; }

        public string? emailid { get; set; }
        public string? firstname { get; set; }
        public string? lastname { get; set; }
        public string? SIDesig { get; set; }
        public Int64? orderid { get; set; }

        public string? SIName { get; set; }

        public string? SIMobile { get; set; }
        public string? DEPEMAIL { get; set; }
        public Int64? roleid { get; set; }
        public string? rolename { get; set; }
        public Int64? warehouseid { get; set; }
        public Int64? districtid { get; set; }
        public string? HimisDistrictid { get; set; }
        public string? BLID { get; set; }
        
    }
}
