
using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.HODTO
{
    public class DirectorateGroupAI_PODTO
    {

        [Key]
        public Int64? GROUPID { get; set; }

        public string? GROUPNAME { get; set; }
        public Int64? NOSINDENT { get; set; }
        public Int64? POGIVEN { get; set; }
        public Double? POVALUE { get; set; }
        public Int64? ITEMSRECEIVED { get; set; }
        public Double? RVALUE { get; set; }

    
    }
}
