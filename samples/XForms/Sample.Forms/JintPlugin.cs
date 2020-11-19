using System;
using Jint;
using Sancho.Client.Core;

namespace Sample.Forms
{
    public class Dummy
    {
        public string Name { get; set; } = "name";
    }

    public class JintPlugin : IPlugin
    {
        private readonly Connection _connection;
        private readonly Engine _engine;

        public string Name => "jint";

        public JintPlugin(Connection connection)
        {
            _connection = connection;

            _engine = new Engine(cfg => cfg.AllowClr(typeof(Dummy).Assembly))
                .SetValue("log", new Action<object>(Console.WriteLine))
                .SetValue("x", new Dummy());
        }

        public void Recieve(Message message)
        {
            switch (message.command)
            {
                case "execute":
                    try
                    {
                        var result = _engine
                        .Execute((string)message.data)
                        .GetCompletionValue()
                        .ToObject();
                        _connection.SendAsync(Name, "execute.result", result, message.metadata.messageId);
                    }
                    catch (Exception ex)
                    {
                        _connection.SendAsync(Name, "execute.error", ex.ToString(), message.metadata.messageId);
                    }
                    break;
            }
        }
    }
}