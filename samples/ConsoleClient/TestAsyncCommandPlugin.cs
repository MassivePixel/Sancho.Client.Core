using System.Threading.Tasks;
using Sancho.Client.Core;
using Serilog;

namespace ConsoleClient
{
    class TestAsyncCommandPlugin : IPlugin
    {
        private Connection _connection;

        public TestAsyncCommandPlugin(Connection connection)
        {
            _connection = connection;
        }

        public string Name => "test.async";

        public async void Recieve(Message message)
        {
            if (message.metadata.origin != "server")
                return;

            Log.Information($"Async command: {message.command}, {message.data}, {message.metadata.messageId}");

            await Task.Delay(2000);

            await _connection.SendAsync(Name, "echo", message.data, message.metadata.messageId);
        }
    }
}
