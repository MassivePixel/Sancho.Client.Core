using System.Threading.Tasks;

namespace Sancho.Client.Core
{
    public interface IConnection
    {
        Task SendAsync(string pluginId, string command, object data = null, string prevMessageId = null);
    }
}
