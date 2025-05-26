
using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.Models
{
    public class PipelineDetailsDTO
    {
 
        [Key]
        public Int64 PONOID { get; set; }
        public Int64 ITEMID { get; set; }
        public string? SUPPLIERNAME { get; set; }
        public string? PHONE1 { get; set; }
        public string? EMAIL { get; set; }
        public string? ITEMTYPENAME { get; set; }
        public string? GROUPNAME { get; set; }
        
            
        public string? ITEMCODE { get; set; }
        public string? ITEMNAME { get; set; }
        public string? STRENGTH1 { get; set; }
        public string? EDLTYPE { get; set; }

        public string? NABLREQ { get; set; }
        public string? PONO { get; set; }
        public string? SOISSUEDATE { get; set; }
        public string? DAYS { get; set; }
        public string? EXTENDEDDATE { get; set; }
        public string? ABSQTY { get; set; }
        
        public string? DISQTY { get; set; }
        public string? EXPECTEDDELIVERYDATE { get; set; }
        public string? RECEIPTABSQTY { get; set; }
        public string? RECPER { get; set; }
        public string? READY { get; set; }
        public string? UQC { get; set; }
        public string? ST { get; set; }
        public string? PIPELINEQTY { get; set; }
        public Double? PIPELINEVALUE { get; set; }
        
        public string? UNIT { get; set; }

        public string? PROGRESS { get; set; }
        public string? REMARKS { get; set; }
        public string? ENTRYDATE { get; set; }




    }
}
