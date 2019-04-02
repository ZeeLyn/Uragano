using System.Threading;
using System.Threading.Tasks;

namespace Uragano.Abstractions.ServiceDiscovery
{
    public interface IServiceStatusManage
    {
        Task Refresh(CancellationToken cancellationToken);
    }
}
