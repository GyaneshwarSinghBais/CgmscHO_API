
using System.ComponentModel.DataAnnotations;
namespace CgmscHO_API.FinanceDTO
{
    public class GrossPaidDetails
    {
        [Key]

        public Int64 ponoid { get; set; }
        public string? itemcode { get; set; }
        public string? itemname { get; set; }
        public string? strength1 { get; set; }
        public string? unit { get; set; }
        public Decimal? finalrategst { get; set; }
        public string? budgetname { get; set; }
        public string? suppliername { get; set; }
        public string? pono { get; set; }
        public string? podate { get; set; }
        public Int64? orderedqty { get; set; }
        public Decimal? POvalue { get; set; }
        public string? RECEIPTDATE { get; set; }
        public Int64? RQTY { get; set; }
        public Decimal? RValue { get; set; }
        public string? ChequeDT { get; set; }
        public Decimal? GrossPaid { get; set; }
        public Decimal? Admin { get; set; }
        public string? ChequeNo { get; set; }
        public Int64? budgetid { get; set; }

        public string? schemename { get; set; }
        public string? SANCTIONDATE { get; set; }
        public string? IndentYear { get; set; }
        public string? PROGRAM { get; set; }
        public string? FMRCODE { get; set; }
        

    }
}
