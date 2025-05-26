
using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.HodDTO
{
    public class WarehouseInfoDTO
    {



        [Key]
                public Int64? WAREHOUSEID { get; set; }
        public String WAREHOUSENAME { get; set; }
        public Int64? NOSFAC { get; set; }
        public String ADDRESS { get; set; }
        public String EMAIL { get; set; }

        public String LATITUDE { get; set; }
        public String LONGITUDE { get; set; }
        public Int64? NOSVEHICLE { get; set; }
        public Int64? NOSDIST { get; set; }
        public String MOB1 { get; set; }
    }
   
}
