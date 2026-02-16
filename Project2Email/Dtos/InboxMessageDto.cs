namespace Project2Email.Dtos
{
    public class InboxMessageDto
    {
        public int MessageId { get; set; }
        public string SenderEmail { get; set; }
        public string ReceiverEmail { get; set; }
        public string Subject { get; set; }
        public string Summary { get; set; }
        public string MessageDetail { get; set; }
        public DateTime SendDate { get; set; }

        public bool IsRead { get; set; }
        public bool IsStarred { get; set; }

        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
    }
}