using System;

namespace ConsoleConnectionTest
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            DoAsync();
            Console.ReadKey();
        }

        async static void DoAsync()
        {
            Sancho.Client.Core.Connection.ProtocolUrl = "http://sanchoprotocol-dev.azurewebsites.net/signalr";

            var connection = new Sancho.Client.Core.Connection("00000000-0000-0000-0000-000000000000");
            Console.WriteLine($"{connection.DeviceId} connecting!");

            var status = await connection.ConnectAsync();
            Console.WriteLine($"Connected: {status}");

            if (status)
            {
                using (connection.On<string>("GotId", id => Console.WriteLine($"Got id '{id}'")))
                {
                    var result = await connection.Invoke<string>("GetId");
                    Console.WriteLine(result);

                    await connection.SendAsync("test", "test");
                }
            }

            connection.Disconnect();
        }
    }
}
