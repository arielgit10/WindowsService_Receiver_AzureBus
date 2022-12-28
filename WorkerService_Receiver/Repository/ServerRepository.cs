using Microsoft.EntityFrameworkCore;
using WorkerService_Receiver.Models;
using WorkerService_Receiver.Repository;

namespace WorkerService_Receiver
{
    public class ServerRepository : IServerRepository
    {
        private AppDbContext _context;
        private DbContextOptions<AppDbContext> GetAllOptions()
        {
            DbContextOptionsBuilder<AppDbContext> optionsBuilder =
                            new DbContextOptionsBuilder<AppDbContext>();

            optionsBuilder.UseSqlServer(AppSettings.ConnectionString);
            return optionsBuilder.Options;
        }
        public async Task PostAccount(Account account)
        {
            using (_context = new AppDbContext(GetAllOptions()))
            {
                try
                {
                    _context.AccountsReceived.Add(account);
                    await _context.SaveChangesAsync();
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

 
    }
}