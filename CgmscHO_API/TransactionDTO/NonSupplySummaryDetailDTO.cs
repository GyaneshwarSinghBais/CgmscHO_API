namespace CgmscHO_API.TransactionDTO
{
    public class NonSupplySummaryDetailDTO
    {
        public Int32? SchemeId { get; set; }
        public string? TenderNo { get; set; }
        public string? TenderName { get; set; }

        public string? ItemCode { get; set; }
        public string? ItemName { get; set; }
        public string? Strength1 { get; set; }
        public string? Unit { get; set; }
        public string? MCategory { get; set; }
        public string? EDLType { get; set; }

        public Int32? PONoId { get; set; }
        public string? PONo { get; set; }
        public string? PODATE { get; set; }          // kept as string since you used TO_CHAR
        public string? ExtendedDate { get; set; }    // kept as string for formatted date

        public Int32? POQty { get; set; }
        public Int32? ReceiptQty { get; set; }
        public Int32? PipelineQty { get; set; }
        public decimal? SupplyPer { get; set; }

        public Int32? Duration { get; set; }
        public Int32? SupplierId { get; set; }
        public string? SupplierName { get; set; }
        public Int32? NoOfDays { get; set; }
    }
}
