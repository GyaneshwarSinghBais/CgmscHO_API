using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.Models
{
    public class ItemDetailDDLDTO
    {   
        [Key]

        public Int64 ITEMID { get; set; }
        public string? MCATEGORY { get; set; }
        public string? ITEMNAME { get; set; }
        public string? STRENGTH1 { get; set; }
        public string? UNIT { get; set; }       
        public Int64 NOSWH { get; set; }
        public Int64 READYFORISSUE { get; set; }
        public Int64 UQCQTY { get; set; }
        public Int64 NOSBATCHES { get; set; }
        public string? LASTMRCDT { get; set; }
        public string? DETAILID { get; set; }

    }
}
