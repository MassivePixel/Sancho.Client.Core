namespace Sancho.Client.Core
{
    public class Message
    {
        public string command { get; set; }
        public object data { get; set; }

        public MessageMetadata metadata { get; set; }
    }
}
