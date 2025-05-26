

using System.ComponentModel.DataAnnotations;

namespace CgmscHO_API.HodDTO
{
    public class SeasonIssueDTO
    {


        [Key]
                public Int64? ID { get; set; }
        public Int64? ISSUEDINNOSAVGSEASON { get; set; }
        public Double? SEASONISSUEDLACS { get; set; }
        public Double? DHSAILacs { get; set; }
        public Double? THISYRISSUEDLACS { get; set; }
        public Double? STKLACS { get; set; }

        public String? ISSUETYPE { get; set; }
        public String? SEASON { get; set; }
        public Int64? ITEMID { get; set; }
        public String? ITEMCODE { get; set; }
        public String?  ITEMNAME { get; set; }

        public String? STRENGTH1 { get; set; }
        public String? ITEMTYPENAME { get; set; }
        public Int64? READY { get; set; }
        public Int64? UQC { get; set; }
 
    }
   
}
