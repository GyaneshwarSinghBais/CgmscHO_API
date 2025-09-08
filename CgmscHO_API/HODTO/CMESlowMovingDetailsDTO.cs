namespace CgmscHO_API.HODTO
{
    public class CMESlowMovingDetailsDTO
    {
        public string? facilityname { get; set; }
        public string? mcategory { get; set; }
        public string? itemcode { get; set; }
        public string? itemname { get; set; }
        public string? strength1 { get; set; }
        public string? unit { get; set; }

        public int? AI { get; set; }
        public int? issueqty { get; set; }
        public decimal? IssuePer { get; set; }
        public int? BalanceAnnualIndentQTY { get; set; }
        public int? WarehouseReadystock { get; set; }
        public decimal? WHREadyPerAgainstBalindent { get; set; }
        public int? facilityid { get; set; }
        public int? itemid { get; set; }
        public string? Stockparameter { get; set; }
        public int? warehouseid { get; set; }
    }
}
