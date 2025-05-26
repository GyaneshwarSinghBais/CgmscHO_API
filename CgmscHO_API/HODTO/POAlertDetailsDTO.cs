
using MessagePack;
using System.ComponentModel.DataAnnotations;
using System.IO.Pipelines;
using System.Net.NetworkInformation;
using System.Security.Cryptography;

namespace CgmscHO_API.Models
{
    public class POAlertDetailsDTO
    {

   

        public string? ID { get; set; }
        public Int64? ITEMID { get; set; }
       
        
        public string? ITEMCODE { get; set; }
        public string? ITEMNAME { get; set; }
        public string? STRENGTH1 { get; set; }
        public string? UNIT { get; set; }

        public Int64 DHSAI { get; set; }
        public Int64 POQTY { get; set; }
        public Int64 BALANCEINDENTPO { get; set; }
        public Int64 READYFORISSUE { get; set; }
        public Int64 UNDERQC { get; set; }
        public Int64 PIPELINEQTY { get; set; }
        public Int64 DHS3MONTHAI { get; set; }
        public Int64 QAUTER1_MSTOCK { get; set; }
        public Int64 TOBEPO { get; set; }
        public Int64 ACTUALTOBEPO { get; set; }

        public string? STOCKOUT { get; set; }
        public string? RCSTATUS { get; set; }
        public string? TOBEPOCOUNT { get; set; }

        public string? EDLTYPEVALUE { get; set; }
        public string? RCSTATUSVALUE { get; set; }
        public string? ITEMTYPENAME { get; set; }
        public string? QCDAYSLAB { get; set; }
        public string? SUP { get; set; }

        public string? RCRATE { get; set; }
        public string? RCSTARTDT { get; set; }
        public string? RCENDDT { get; set; }
        

        public string? NEDL { get; set; }

        public string? EDL { get; set; }
        public string? EDLTYPE { get; set; }
   

    }
}
