namespace Project2Email.Entities
{
    public class Category
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } 
        public string? Icon { get; set; }       
        public string? ColorCode { get; set; }   

        public ICollection<Message> Messages { get; set; }
    }
}