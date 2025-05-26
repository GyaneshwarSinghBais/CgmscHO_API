
using System.ComponentModel.DataAnnotations;


namespace CgmscHO_API.Models
{
    public class FitDetailDTO
    {
        [Key]
        public Int64 PONOID { get; set; }
        public string? PAYMENTREQUIRED { get; set; }
        public string? SUPPLIERNAME { get; set; }
        public string? ITEMCODE { get; set; }
        public string? ITEMNAME { get; set; }
        public string? PONO { get; set; }
        public string? HASFILE { get; set; }
        public string? FILEID { get; set; }
        public string? PODATE { get; set; }
        public string? POQTY { get; set; }
        public string? TOTALPOVALUE { get; set; }
        public string? RECEIPTQTY { get; set; }
        public string? MRCDATE { get; set; }
        public string? LASTMRCDT { get; set; }
        public string? LASTQCPAADEDDT { get; set; }
        public string? RECEIPTVALUE { get; set; }
        public string? LIBWITHOUTADM { get; set; }
        public string? LIBWITHADM { get; set; }
        public string? TORECEPER { get; set; }
        public string? PRESENTFILE { get; set; }
        public string? REASONNAME { get; set; }
        public string? FITUNFIT { get; set; }
        public string? HODTYPE { get; set; }
        public string? FILENO { get; set; }
        public string? FILEDT { get; set; }
        public string? SOURCE { get; set; }
        public string? SDDATE { get; set; }
        public string? PHYRECEIPTDT { get; set; }
        public string? SDID { get; set; }
        public string? FITDATE { get; set; }
        public string? MONTHDATE { get; set; }
        public string? MONTHNUMBER { get; set; }
        public string? SANCTIONSTATUS { get; set; }

    }
}
