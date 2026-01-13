namespace CgmscHO_API.TransactionDTO
{
    public class DmeFacNocSummaryDTO
    {
        public string? mcategory { get; set; }
        public string? facilityname { get; set; }

        public int? EDL_CNT { get; set; }
        public decimal? EDL_VAL { get; set; }

        public int? NON_EDL_CNT { get; set; }
        public decimal? NON_EDL_VAL { get; set; }

        public string? districtname { get; set; }
        public long? facilityid { get; set; }

    }
}
