namespace DGNet.Client;

public interface IGDNetBackend
{

}

public class Client
{
    private IGDNetBackend? _backend = null;
}

public sealed class ENetBackend : IGDNetBackend
{
    private static bool _initialized = false;

    public static void EnsureInit()
    {
        if (_initialized == false)
        {
            ENet.Library.Initialize();
            _initialized = true;
        }
    }

    public sealed class ENetClient : IDisposable
    {
        private readonly CancellationTokenSource _tokenSource;
        private readonly Thread _thread;

        public ENetClient()
        {
            _tokenSource = new();
            _thread = new(() => ThreadStart(_tokenSource.Token));
        }

        public void Dispose()
        {
            _tokenSource.Cancel();
            _thread.Join();
        }

        private static void ThreadStart(CancellationToken cancellationToken)
        {
            
        }
    }

    public sealed class ENetServer : IDisposable
    {
        // TODO: Not hardcode this...
        private const int PEER_LIMIT = 16;

        private readonly CancellationTokenSource _tokenSource;

        private readonly Thread _thread;

        public ENetServer()
        {
            _tokenSource = new();
            _thread = new(() => ThreadStart(_tokenSource.Token));
        }

        private void Run()
        {
            _thread.Start();
        }

        public void Dispose()
        {
            _tokenSource.Cancel();
            _thread.Join();
        }

        private static void ThreadStart(CancellationToken cancellationToken)
        {
            using ENet.Host host = new();
            ENet.Address address = new()
            {
                Port = 8184 // TODO: Not hardcode this either...
            };

            host.Create(address, PEER_LIMIT);

            while (!cancellationToken.IsCancellationRequested)
            {
                ThreadPoll(host);
            }
        }

        private static void ThreadPoll(ENet.Host host)
        {
            bool polled = false;
            while (!polled)
            {
                if (host.CheckEvents(out ENet.Event @event) <= 0)
                {
                    if (host.Service(15, out @event) <= 0)
                    {
                        break;
                    }
                    polled = true;
                }

                switch (@event.Type)
                {
                    case ENet.EventType.Connect:
                        break;
                    case ENet.EventType.Disconnect:
                        break;
                    case ENet.EventType.Receive:
                        @event.Packet.Dispose();
                        break;
                    case ENet.EventType.Timeout:
                        break;
                    default:
                        break;
                }
            }
        }

    }


}