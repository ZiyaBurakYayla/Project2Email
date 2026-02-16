namespace Project2Email.Entities
{
    public class Message
    {
        public int MessageId { get; set; }
        public string SenderEmail { get; set; }
        public string ReceiverEmail { get; set; }
        public string Subject { get; set; }
        public string MessageDetail { get; set; }
        public DateTime SendDate { get; set; }

        public string? Summary { get; set; }      
        public int? CategoryId { get; set; }      
        public string Category { get; set; }

        public ICollection<UserMessageState> UserMessageStates { get; set; }
    }
}