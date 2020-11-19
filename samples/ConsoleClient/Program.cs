using System;
using System.Threading.Tasks;
using Sancho.Client.Core;
using Serilog;

namespace ConsoleClient
{
    class Program
    {
        static Connection conn;

        static async Task Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .CreateLogger();

            await MainAsync();
            Console.ReadKey();
            await conn?.DisconnectAsync();
        }

        static async Task MainAsync()
        {
            conn = new Connection();
            conn.AddPlugin(new EchoPlugin(conn));
            conn.AddPlugin(new TestAsyncCommandPlugin(conn));
            conn.AddPlugin(new JintPlugin(conn));

            if (!await conn.ConnectAsync(info: "console"))
            {
                Log.Error("Failed to connect!");
            }
        }
    }
}
