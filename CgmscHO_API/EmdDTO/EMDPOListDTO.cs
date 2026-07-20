namespace CgmscHO_API.EmdDTO
{
    public class EMDPOListDTO
    {
        public int MCID { get; set; }
        public string Category { get; set; }
        public decimal PONOID { get; set; }
        public string PONO { get; set; }
        public DateTime? PODate { get; set; }
        public string FileNo { get; set; }
        public int? BudgetId { get; set; }
        public decimal? SanctionId { get; set; }
        public string SanctionNo { get; set; }
        public DateTime? SanctionDate { get; set; }
        public decimal? GrossAmount { get; set; }
        public string SupplierName { get; set; }
    }
}
