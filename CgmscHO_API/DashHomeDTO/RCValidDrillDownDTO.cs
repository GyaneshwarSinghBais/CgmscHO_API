namespace CgmscHO_API.DashHomeDTO
{
    public class RCValidDrillDownDTO
    {
        public int ItemId { get; set; }
        public string? ItemCode { get; set; }
        public string? ItemName { get; set; }
        public string? Sku { get; set; }       // m.unit
        public int? UnitCount { get; set; }

        public string? EdlType { get; set; }   // 'EDL' or 'Non EDL'

        public decimal? DhsAiQty { get; set; } // (DHS_INDENTQTY + MITANIN)
        public decimal? DmeAiQty { get; set; }

        public string? RcEndDate { get; set; }
        public decimal? RcRate { get; set; }
        public int? NoOfSuppliers { get; set; }

        public string? TenderStatus { get; set; }
        public string? ActionCode { get; set; }
    }
}
