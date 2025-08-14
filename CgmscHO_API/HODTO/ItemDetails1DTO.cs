namespace CgmscHO_API.HODTO
{
    public class ItemDetails1DTO
    {
        public int? itemid { get; set; }
        public string? itemcode { get; set; }
        public string? itemname { get; set; }
        public string? strength1 { get; set; }
        public string? unit { get; set; }
        public int? mcid { get; set; }
        public string? MCATEGORY { get; set; }
        public string? RCEDL { get; set; }  // 'Valid' or 'Not Valid'
    }
}
