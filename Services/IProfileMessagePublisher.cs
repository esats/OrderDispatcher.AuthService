using System.Threading;
using System.Threading.Tasks;
using OrderDispatcher.AuthService.Models;

namespace OrderDispatcher.AuthService.Services
{
    public interface IProfileMessagePublisher
    {
        Task PublishProfileCreatedAsync(ProfileModel profile, CancellationToken ct = default);
    }
}
