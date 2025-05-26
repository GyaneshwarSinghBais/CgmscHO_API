using System.ComponentModel.DataAnnotations;
namespace CgmscHO_API.QCDTO
{
    public class QCHomeDashPlacewise
    {
        

        [Key]
        public Int64 nositems { get; set; }
   
        public Decimal? STKVALUE { get; set; }
        public Int64? nosbatch { get; set; }
        public Int64? QDIssuePendingbyWH { get; set; }
        public Int64? WHIssueButPendingInCourier { get; set; }
        public Int64? HOQC_LabIssuePending { get; set; }
        public Int64? DropPendingToLab { get; set; }
        public Int64? LabAnalysisOngoing { get; set; }

        public Int64? PendingforfinalUpdate { get; set; }
    }
}
