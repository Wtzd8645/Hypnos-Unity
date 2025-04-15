namespace Blanketmen.Hypnos.Network
{
    internal interface ISocket
    {
        public uint Id { get; }
        public uint Version { get; }

        public void Start();
        public void Stop();

        public void Dispatch();
        public void Process(IOEventArgs args);
    }
}