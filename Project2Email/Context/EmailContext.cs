using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Project2Email.Entities;

namespace Project2Email.Context
{
    public class EmailContext : IdentityDbContext<AppUser>
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("server=ZIYABURAKYAYLA\\SQLEXPRESS;initial catalog=P2Email;integrated security=true");
        }

        public DbSet<Message> Messages { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<UserMessageState> UserMessageStates { get; set; }

    }
}
