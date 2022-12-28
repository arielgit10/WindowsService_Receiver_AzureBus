using Microsoft.EntityFrameworkCore;
using WorkerService_Receiver.Models;

namespace WorkerService_Receiver.Repository
{
    public class AppDbContext : DbContext
    {
        public AppDbContext() {}
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public virtual DbSet<Account> AccountsReceived { get; set; } = null!;
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Account>(account =>
            {
                account.ToTable("AccountsReceived");
                account.HasKey(a => a.AccountId);
                account.Property(a => a.AccountId).ValueGeneratedOnAdd();
                account.Property(a => a.Alias).HasMaxLength(27);
                account.Property(a => a.Cbu).HasMaxLength(22);
                account.Property(a => a.Balance);

            });
        }

    }
}
