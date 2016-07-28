// Copyright (c) Massive Pixel.  All Rights Reserved.  Licensed under the MIT License (MIT). See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client;
using Sancho.Client.Core.Helpers;

namespace Sancho.Client.Core
{
    public class Connection : IConnection
    {
        public static string ProtocolUrl { get; set; }

        public string ApiKey { get; }
        public string DeviceId { get; }

        HubConnection hubConnection;
        List<IPlugin> plugins = new List<IPlugin>();
        IHubProxy proxy;

        Queue<Message> pending = new Queue<Message>();

        public Connection(string apiKey)
        {
            if (string.IsNullOrEmpty(apiKey))
                throw new ArgumentNullException(nameof(apiKey));

            ApiKey = apiKey;

            DeviceId = Settings.DeviceId;
            if (string.IsNullOrWhiteSpace(DeviceId))
            {
                DeviceId = Settings.DeviceId = Guid.NewGuid().ToString();
            }
        }

        public async Task<bool> ConnectAsync()
        {
            try
            {
                hubConnection = new HubConnection(ProtocolUrl);
                hubConnection.Headers.Add("ApiKey", ApiKey);
                hubConnection.Headers.Add("Id", DeviceId);
                hubConnection.Headers.Add("Type", "client");

                proxy = hubConnection.CreateHubProxy("Protocol");

                proxy.On<Message>("Receive", m =>
                {
                    var plugin = plugins.FirstOrDefault(x => x.Name == m?.metadata?.pluginId);
                    plugin?.Recieve(m);
                });

                await hubConnection.Start();

                return true;
            }
            catch (Exception ex)
            {
                var msg = ex.ToString();
#if DEBUG
                Debug.WriteLine(msg);
#endif
                return false;
            }
        }

        public void Disconnect()
        {
            hubConnection?.Stop();
        }

        public IDisposable On<T>(string eventName, Action<T> onData) => proxy.On(eventName, onData);
        public Task Invoke(string method, params object[] args) => proxy.Invoke(method, args);
        public Task<T> Invoke<T>(string method, params object[] args) => proxy.Invoke<T>(method, args);

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

        Task DoSend(Message message)
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

            return proxy.Invoke("Send", message);
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
