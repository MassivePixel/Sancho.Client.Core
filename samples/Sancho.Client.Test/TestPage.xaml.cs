using Sancho.Client.Core;
using Xamarin.Forms;

namespace Sancho.Client.Test
{
    public partial class TestPage : ContentPage
    {
        Connection connection;

        public TestPage()
        {
            InitializeComponent();

            Connection.ProtocolUrl = "http://sanchoprotocol-dev.azurewebsites.net/";
            connection = new Connection("00000000-0000-0000-0000-000000000000");
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
