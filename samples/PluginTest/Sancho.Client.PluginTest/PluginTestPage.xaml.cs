using Sancho.Client.Core;

namespace Sancho.Client.PluginTest
{
    class TestPlugin : IPlugin
    {
        IConnection connection;

        public string Name => "test";

        public TestPlugin(IConnection connection)
        {
            this.connection = connection;
        }

        public async void Recieve(Message message)
        {
            await connection.SendAsync(Name, "hi");
        }
    }

    public partial class PluginTestPage
    {
        Connection connection;

        public PluginTestPage()
        {
            InitializeComponent();

            Connection.ProtocolUrl = "http://sanchoprotocol-dev.azurewebsites.net/";
            connection = new Connection("00000000-0000-0000-0000-000000000000");
            connection.AddPlugin(new TestPlugin(connection));
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            connectingStatus.IsRunning = true;

            try
            {
                var connected = await connection.ConnectAsync();
                connectionStatus.Text = connected ? "connected!" : "failed to connect";
            }
            finally
            {
                connectingStatus.IsRunning = false;
            }
        }
    }
}
