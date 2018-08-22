using Sancho.Client.Core;
using Serilog;
using Xamarin.Forms;

namespace Sample.Forms
{
    class EchoPlugin : IPlugin
    {
        private Connection _connection;

        public EchoPlugin(Connection connection)
        {
            _connection = connection;
        }

        public string Name => "echo";

        public void Recieve(Message message)
        {
            Log.Information($"Received message {message.command} {message.data} from {message.metadata.senderId}");

            _connection.SendAsync(Name, message.command, message.data, message.metadata.messageId);
        }
    }

    public partial class MainPage : ContentPage
    {
        private readonly Connection _connection;

        public MainPage()
        {
            InitializeComponent();
            _connection = new Connection();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            _connection.AddPlugin(new EchoPlugin(_connection));
            //_connection.AddPlugin(new TestAsyncCommandPlugin(_connection));
            //_connection.AddPlugin(new JintPlugin(_connection));

            if (await _connection.ConnectAsync("http://192.168.0.12:5000"))
            {
                //conn.On<Message>("receive", (message) =>
                //{
                //    Log.Information($"Received! {message.command}");
                //});

                await _connection.SendAsync("echo", "hi", "from Xamarin");
                status.Text = "Connected";
            }
            else
            {
                status.Text = "NOT CONNECTED";
                status.TextColor = Color.Red;
                Log.Error("Failed to connect!");
            }
        }
    }
}
