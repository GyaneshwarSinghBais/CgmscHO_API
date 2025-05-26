using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.DTO
{
    public class GetFileStorageLocationDTO
    {
        [Key]
        public Int64 RACKID { get; set; }
        public String? LOCATIONNO { get; set; }
    }
}
