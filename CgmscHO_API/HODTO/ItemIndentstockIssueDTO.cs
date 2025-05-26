

using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.Models
{
    public class ItemIndentstockIssueDTO
    {

   


       [Key]
        public Int64 ITEMID { get; set; }
        public Int64 ITEMTYPEID { get; set; }
        public Int64 GROUPID { get; set; }

        public string? GROUPNAME { get; set; }
        public string? ITEMTYPENAME { get; set; }
        public string? ITEMCODE { get; set; }
        public string? ITEMNAME { get; set; }
        public string? STRENGTH1 { get; set; }
        public string? UNIT { get; set; }
        public Int64 TOTALAI { get; set; }
        public Int64 DHSAI { get; set; }
        public Int64 DMEAI { get; set; }

        public Int64 READYSTOCK { get; set; }
        public Int64 QCSTOCK { get; set; }
        public Int64 TOTALPIPLIE { get; set; }
        public string? EDLTYPE { get; set; }
        public string? EDL { get; set; }
        public string? RCSTATUS { get; set; }
        public string? RCRATE { get; set; }
        public string? RCSTARTDT { get; set; }
        public string? RCENDDT { get; set; }



        public Int64 DMEISSUE { get; set; }
        public Int64 DHSISSUE { get; set; }
        public Int64 TOTALISSUE { get; set; }



        public Int64 DHSPOQTY { get; set; }
        public Int64 DHSRQTY { get; set; }
        public Int64 DMEPOQTY { get; set; }
        public Int64 DMERQTY { get; set; }


        public Int64 RC { get; set; }
        public Int64 READYCNT { get; set; }
        public Int64 UQCCOUNT { get; set; }
        public Int64 PIPLINECNT { get; set; }

        




    }
}
