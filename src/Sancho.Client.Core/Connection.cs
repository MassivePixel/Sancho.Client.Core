using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client;

namespace Sancho.Client.Core
{
    public class Connection : IConnection
    {
        public static string ProtocolUrl { get; } = "http://sanchoprotocol-dev.azurewebsites.net/";

        public string ApiKey { get; }
        public string DeviceId { get; private set; }

        HubConnection hubConnection;
        List<IPlugin> plugins = new List<IPlugin>();
        IHubProxy proxy;

        Queue<Message> pending = new Queue<Message>();

        public Connection(string apiKey)
        {
            if (string.IsNullOrEmpty(apiKey))
                throw new ArgumentNullException(nameof(apiKey));

            ApiKey = apiKey;
        }

        public async Task ConnectAsync()
        {
            try
            {
                hubConnection = new HubConnection(ProtocolUrl);
                proxy = hubConnection.CreateHubProxy("Protocol");
                proxy.On<string>("SetId", id =>
                {
                    Debug.WriteLine($"Id set to {id}");
                    DeviceId = id;

                    while (pending.Any())
                    {
                        var message = pending.Dequeue();
                        message.metadata.senderId = DeviceId;
                        message.metadata.origin = "client";
                        proxy.Invoke("Send", ApiKey, message);
                    }
                });

                proxy.On<Message>("Receive", m =>
                {
                    var plugin = plugins.FirstOrDefault(x => x.Name == m?.metadata?.pluginId);
                    plugin?.Recieve(m);
                });

                await hubConnection.Start();

                await proxy.Invoke("RegisterDevice", ApiKey, string.Empty);
            }
            catch (Exception ex)
            {
                var msg = ex.ToString();
            }
        }

        public Task SendAsync(string pluginId, string command, object data = null, string prevMessageId = null)
            => DoSend(new Message
            {
                command = command,
                data = data,
                metadata = new MessageMetadata
                {
                    pluginId = pluginId,
                    senderId = DeviceId,
                    origin = "client"
                }
            });

        private Task DoSend(Message message)
        {
            if (message == null)
            {
                Debug.WriteLine("Warning: Sending null message");
                return Task.FromResult(false);
            }

            if (proxy == null)
            {
                pending.Enqueue(message);
                return Task.FromResult(false);
            }

            return proxy.Invoke("Send", ApiKey, message);
        }

        public void AddPlugin(IPlugin plugin)
        {
            if (plugin == null)
                throw new ArgumentNullException(nameof(plugin));

            if (plugins.Any(x => x.Name == plugin.Name))
                throw new InvalidOperationException($"Cannot add multiple plugins with the same name ('{plugin.Name}'");

            Debug.WriteLine($"Plugin '{plugin.Name}' added.");
            plugins.Add(plugin);
        }

        public void RemovePlugin(IPlugin plugin)
        {
            if (plugin == null) throw new ArgumentNullException(nameof(plugin));

            Debug.WriteLine($"Plugin '{plugin.Name}' removed.");
            plugins.Remove(plugin);
        }

        public void RemovePlugin(string name)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));

            Debug.WriteLine($"Plugin '{name}' removed.");
            plugins.RemoveAll(x => x.Name == name);
        }
    }
}
