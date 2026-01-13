namespace CgmscHO_API.FinanceDTO
{
    public class getFitUnfitSummaryDTO
    {
        public string? SECTIONNAME { get; set; }

        // Fund Head / Budget Name (nullable string)
        public string? FUNDHEAD { get; set; }

        // Number of POs (nullable int)
        public int? NOSPO { get; set; }

        // Receipt value in Lacs (nullable decimal)
        public decimal? RECVALUELACS { get; set; }
    }
}
