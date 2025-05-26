namespace CgmscHO_API.HODTO
{
    public class ConversationHodCgmscDTO
    {
        public int? SchemeId { get; set; }
        public string? SchemeName { get; set; }
        public string? HOD { get; set; }
        public string? LetterNo { get; set; }
        public string? LetterDate { get; set; }
        public string? Remarks { get; set; }
        public string? SendDate { get; set; }
        public DateTime? EntryDate { get; set; }
        public string? FileName { get; set; }
        public string? FilePath { get; set; }
        public int? ConvId { get; set; }

        // Reply Section
        public string? RecvDate { get; set; }
        public string? ReplyLetterNo { get; set; }
        public string? ReplyLetterDT { get; set; }
        public string? ReplyRemarks { get; set; }
        public string? ReplyFileName { get; set; }
        public string? ReplyFilePath { get; set; }
    }
}
