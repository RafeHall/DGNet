using DGNet.Client;

using ENetLib = ENet;

namespace DGNet.ENet;

public sealed class ENetBackend : IGDNetBackend
{
    private static bool _initialized = false;

    public static void EnsureInit()
    {
        if (_initialized == false)
        {
            ENetLib.Library.Initialize();
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
            using ENetLib.Host host = new();
            ENetLib.Address address = new()
            {
                Port = 8184 // TODO: Not hardcode this either...
            };

            host.Create(address, PEER_LIMIT);

            while (!cancellationToken.IsCancellationRequested)
            {
                ThreadPoll(host);
            }
        }

        private static void ThreadPoll(ENetLib.Host host)
        {
            bool polled = false;
            while (!polled)
            {
                if (host.CheckEvents(out ENetLib.Event @event) <= 0)
                {
                    if (host.Service(15, out @event) <= 0)
                    {
                        break;
                    }
                    polled = true;
                }

                switch (@event.Type)
                {
                    case ENetLib.EventType.Connect:
                        break;
                    case ENetLib.EventType.Disconnect:
                        break;
                    case ENetLib.EventType.Receive:
                        @event.Packet.Dispose();
                        break;
                    case ENetLib.EventType.Timeout:
                        break;
                    default:
                        break;
                }
            }
        }

    }
}