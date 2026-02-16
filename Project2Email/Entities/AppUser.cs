using Microsoft.AspNetCore.Identity;

namespace Project2Email.Entities
{
    public class AppUser : IdentityUser
    {
        public string Name { get; set; }
        public string Surname { get; set; }
        public string? ImageUrl { get; set; }
        public string? About { get; set; }
        public int ConfirmCode { get; set; }

        public ICollection<UserMessageState> UserMessageStates { get; set; }
    }
}
