namespace Project2Email.Dtos
{
    public class UserEditDto
    {
        public string Name { get; set; }    
        public string Surname { get; set; }    
        public string Password { get; set; }    
        public string ConfirmPassword { get; set; }    
        public string Email { get; set; }     
        public string About { get; set; }     
        public string ImageUrl { get; set; }    
        public IFormFile Image { get; set; }    
    }
}
