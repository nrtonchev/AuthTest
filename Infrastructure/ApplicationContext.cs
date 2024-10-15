using Core.Entities.Models;
using Core.Entities.Auth;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure
{
	public class ApplicationContext : DbContext
	{
		public ApplicationContext(DbContextOptions options) : base(options)
		{
		}

        public DbSet<Account> Accounts { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
    }
}
