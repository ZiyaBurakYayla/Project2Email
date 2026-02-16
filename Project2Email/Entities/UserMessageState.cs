namespace Project2Email.Entities
{
    public class UserMessageState
    {
        public int UserMessageStateId { get; set; }

        public int MessageId { get; set; }
        public Message Message { get; set; }

        public string AppUserId { get; set; }
        public AppUser AppUser { get; set; }

        public bool IsRead { get; set; }
        public bool IsStarred { get; set; }
        public bool IsTrash { get; set; }

        public string Folder { get; set; }
    }
}