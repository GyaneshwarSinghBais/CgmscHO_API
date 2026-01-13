namespace CgmscHO_API.FinanceDTO
{
    public class getFitUnfitDTO
    {
        public string? SECTIONNAME { get; set; } = null!;
        public string? PRESENTFILE { get; set; } = null!;
        public string? FundHead { get; set; } = null!;
        public string? SUPPLIERNAME { get; set; } = null!;
        public string? POYear { get; set; } = null!;  // e.g., "2025-2026"
        public string? PONO { get; set; } = null!;
        public string? PODATE { get; set; } = null!; // stored as string (dd-MM-yyyy)
        public string? FMRCODE { get; set; } = null!;
        public string? PROGRAM { get; set; } = null!;
        public string? MCATEGORY { get; set; } = null!;
        public string? ITEMCODE { get; set; } = null!;
        public string? ITEMNAME { get; set; } = null!;
        public string? UNIT { get; set; } = null!;
        public string? STRENGTH1 { get; set; } = null!;
        public int? POQTY { get; set; }
        public decimal? TOTALPOVALUE { get; set; }
        public int? RECEIPTQTY { get; set; }
        public decimal? RECEIPTVALUE { get; set; }
        public string? MRCDATE { get; set; } = null!;
        public string? QCPASSEDDT { get; set; } = null!; // stored as string (dd-MM-yyyy)
        public string? SDDATE { get; set; } = null!;      // stored as string (dd-MM-yyyy or 'Not Received')
        public string? FITUNFIT { get; set; } = null!;
        public string? VALIDITY { get; set; } = null!;
        public int? NSQSTOCK { get; set; }
        public int? HOLDSTOCK { get; set; }
        public string? SUPPENINDSD { get; set; } = null!; // "Yes"/"No"
        public int? PONOID { get; set; }
        public int? ITEMID { get; set; }
        public int? SUPPLIERID { get; set; }
    }
}
