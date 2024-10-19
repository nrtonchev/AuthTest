using Core.Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure
{
	public class ApplicationContext : DbContext
	{
		public ApplicationContext(DbContextOptions options) : base(options)
		{
		}

        public DbSet<Account> Accounts { get; set; }
    }
}
