using System;
using Sancho.Client.Core;
using Serilog;

namespace ConsoleClient
{
    class TestPlugin : IPlugin
    {
        public string Name => "test";

        public void Recieve(Message message)
        {
            Log.Information($"Received message {message.command} {message.data} from {message.metadata.senderId}");
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .CreateLogger();

            MainAsync();
            Console.ReadKey();
        }

        static async void MainAsync()
        {
            var conn = new Connection();
            conn.AddPlugin(new TestPlugin());

            if (await conn.ConnectAsync())
            {
                //conn.On<Message>("receive", (message) =>
                //{
                //    Log.Information($"Received! {message.command}");
                //});

                await conn.SendAsync("test", "hi", "from console");
            }
            else
            {
                Log.Error("Failed to connect!");
            }
        }
    }
}
