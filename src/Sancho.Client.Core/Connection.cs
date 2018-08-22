// Copyright (c) Massive Pixel.  All Rights Reserved.  Licensed under the MIT License (MIT). See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Sancho.Client.Core.Helpers;
using Serilog;

namespace Sancho.Client.Core
{
    /// <summary>
    /// Connection to the Sancho server.
    /// </summary>
    public class Connection : IConnection
    {
        /// <summary>
        /// Underlying SignalR connection.
        /// </summary>
        private HubConnection connection;

        /// <summary>
        /// List of registered plugins available for server.
        /// Used for dispatching incoming messages.
        /// </summary>
        private readonly List<IPlugin> plugins = new List<IPlugin>();

        /// <summary>
        /// All messages are queued before sending.
        /// </summary>
        private readonly Queue<Message> pending = new Queue<Message>();

        /// <summary>
        /// Gets or sets the protocol URL.
        /// </summary>
        /// <value>The protocol URL.</value>
        public static string ProtocolUrl { get; set; }

        /// <summary>
        /// Gets the device identifier.
        /// </summary>
        /// <value>The device identifier.</value>
        public string DeviceId { get; }

        /// <summary>
        /// Gets the hub connection.
        /// </summary>
        /// <value>The hub connection.</value>
        protected HubConnection HubConnection => connection;

        public Connection()
        {
            DeviceId = Settings.DeviceId;
            if (string.IsNullOrWhiteSpace(DeviceId))
            {
                DeviceId = Settings.DeviceId = Guid.NewGuid().ToString();
            }
        }

        public async Task<bool> ConnectAsync(string overrideUrl)
        {
            try
            {
                var url = (overrideUrl ?? "http://localhost:5000")+"/protocol";
                connection = new HubConnectionBuilder()
                    .WithUrl(url)
                    .Build();

                connection.On<Message>("receive", m =>
                {
                    if (m.metadata?.origin != "server")
                    {
                        // drop all non-server messages
                        return;
                    }

                    var plugin = plugins.FirstOrDefault(x => x.Name == m?.metadata?.pluginId);
                    plugin?.Recieve(m);
                    if (plugin == null)
                    {
                        Log.Warning("No plugin matching {PluginId}", m?.metadata?.pluginId);
                    }
                });

                Log.Debug("Connecting to Sancho protocol...");
                await connection.StartAsync();
                Log.Debug("Connected!");

                return true;
            }
            catch (Exception ex)
            {
                var msg = ex.ToString();
#if DEBUG
                Debug.WriteLine(msg);
#endif
                Log.Error(ex, "Error connecting!");
                return false;
            }
        }

        public Task DisconnectAsync()
        {
            Log.Debug("Disconnecting.");
            return connection?.StopAsync();
        }

        public IDisposable On<T>(string eventName, Action<T> onData)
            => connection.On(eventName, onData);
        public IDisposable On<T1, T2>(string eventName, Action<T1, T2> onData)
            => connection.On(eventName, onData);
        public Task Invoke(string method, params object[] args)
            => connection.InvokeAsync(method, args);
        public Task<T> Invoke<T>(string method, params object[] args)
            => connection.InvokeAsync<T>(method, args);

        /// <summary>
        /// Send standard message from plugin to its corresponding server part.
        /// </summary>
        /// <returns>The async.</returns>
        /// <param name="pluginId">Plugin identifier.</param>
        /// <param name="command">Command.</param>
        /// <param name="data">Data.</param>
        /// <param name="prevMessageId">Previous message identifier.</param>
        public Task SendAsync(string pluginId, string command, object data = null, string prevMessageId = null)
            => DoSend(new Message
            {
                command = command,
                data = data,
                metadata = new MessageMetadata
                {
                    pluginId = pluginId,
                    senderId = DeviceId,
                    origin = "client",
                    messageId = prevMessageId
                }
            });

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

        private Task DoSend(Message message)
        {
            if (message == null)
            {
                Log.Warning("Warning: Sending null message");
                return Task.FromResult(false);
            }

            if (connection == null)
            {
                Log.Debug("Not connected, adding to queue {Count}", pending.Count);
                pending.Enqueue(message);
                return Task.FromResult(false);
            }

            Log.Debug("Sending message");
            return connection.InvokeAsync("Send", message);
        }
    }
}
