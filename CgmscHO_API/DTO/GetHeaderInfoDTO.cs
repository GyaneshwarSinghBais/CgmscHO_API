using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.DTO
{
    public class GetHeaderInfoDTO
    {
        [Key]
        public Int64 facreceiptid { get; set; }
        public Int64? sourceid { get; set; }
        public Int64? schemeid { get; set; }
        public Int64? facilityid { get; set; }
        public Int64? indentid { get; set; }
        public Int64? issueid { get; set; }
        public String? facreceiptno { get; set; }
        public String? facreceiptdate { get; set; }
        public String? facreceipttype { get; set; }
        public String? facreceiptvalue { get; set; }
        public String? remarks { get; set; }
        public String? status { get; set; }
        public String? wardid { get; set; }
        public String? supplierid { get; set; }
        public String? reasonflag { get; set; }
        public String? isedl { get; set; }
        public String? tofacilityid { get; set; }
        public String? ponoid { get; set; }
        public String? stkregno { get; set; }
        public String? stkregdate { get; set; }
        public String? mrcnumber { get; set; }
        public String? mrcdate { get; set; }
        public String? sptype { get; set; }
        public String? splocationid { get; set; }
        public String? invoiceno { get; set; }
        public String? invoicedate { get; set; }
        public String? voucherstatus { get; set; }
        public String? isnicshare { get; set; }
        public String? compentrydt { get; set; }
        public String? receiptby { get; set; }
        public String? recbydate { get; set; }
        public String? receivedby { get; set; }
        public String? recbymobileno { get; set; }
        public String? entrydate { get; set; }
        public String? isuseapp { get; set; }


      
    }
}


