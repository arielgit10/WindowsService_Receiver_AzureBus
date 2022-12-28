using WorkerService_Receiver.Models;

namespace WorkerService_Receiver
{
    public interface IServerRepository
    {
        Task PostAccount(Account account);
    }
}