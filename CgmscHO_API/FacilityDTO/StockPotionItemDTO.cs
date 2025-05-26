using System.ComponentModel.DataAnnotations;
namespace CgmscHO_API.Models
{
    public class StockPotionItemDTO
    {
        [Key]

        public string? ITEMCODE { get; set; }
        public string? ITEMNAME { get; set; }
        public string? EDL { get; set; }
        public string? EDLTYPE { get; set; }
        
        public string? STRENGTH1 { get; set; }
        public string? ITEMTYPENAME { get; set; }
        public string? MCATEGORY { get; set; }
        public string? GNAME { get; set; }

        public Int64? READYWH { get; set; }
        public Int64? QCPENDINGWH { get; set; }
        public Int64? TOTLPIPELINE { get; set; }

        public Int64? DISTRICTSTOCK { get; set; }
        public Int64? CMHOAPRAIQTY { get; set; }
        public Int64? DISTFACISSUEQTY { get; set; }
        public Int64? DISTNOCQTY { get; set; }
        public Int64? DISTLPOQTY { get; set; }
        public Int64? DISTWARDISSUEQTY { get; set; }
        public Int64? DISTPATIENTISSUEQTY { get; set; }
        public Int64? NOOFPATIENTS { get; set; }


    }
}
