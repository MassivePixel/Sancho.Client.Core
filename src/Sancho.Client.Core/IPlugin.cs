namespace Sancho.Client.Core
{
    public interface IPlugin
    {
        string Name { get; }

        void Recieve(Message message);
    }
}
