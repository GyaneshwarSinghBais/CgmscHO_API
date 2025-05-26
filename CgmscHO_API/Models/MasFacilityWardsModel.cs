using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CgmscHO_API.Models
{
    [Table("MASFACILITYWARDS")]
    public class MasFacilityWardsModel
    {
        [Key]
        public string? wardid { get; set; }
        public string? pwd { get; set; }
        public string? facilityid { get; set; }
        public string? facilitytypeid { get; set; }
    }
}
