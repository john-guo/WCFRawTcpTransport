using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.ServiceModel;

namespace WCFRawTcpTransport
{
    class CustomTcpSessionChannelListener : ChannelListenerBase<IDuplexSessionChannel>, IPoolManager<AsyncProxy>
    {
        private Uri _uri;
        private TcpListener _listener;
        private MessageEncoderFactory _factory;
        private BindingContext _context;
        private InvokerStub _stub;

        private BufferManager _bufferManager;
        private SimpleObjectPool<AsyncProxy> _asyncPool;
        public BufferManager Buffer
        {
            get
            {
                return _bufferManager;
            }
        }
        public IObjectPool<AsyncProxy> Pool
        {
            get
            {
                return _asyncPool;
            }
        }

        internal CustomTcpSessionChannelListener(BindingContext context, MessageEncoderFactory factory, InvokerStub stub)
            : base(context.Binding)
        {
            _context = context;
            _factory = factory;
            _uri = context.ListenUriBaseAddress;
            _stub = stub;

            _asyncPool = new SimpleObjectPool<AsyncProxy>(CustomTransportConstant.MaxObjectPoolSize, true);
            _bufferManager = BufferManager.CreateBufferManager(CustomTransportConstant.MaxBufferPoolSize, CustomTransportConstant.MaxBufferSize);
        }

        private void StartListen()
        {
            if (_listener != null)
                return;
            try
            {
                IPAddress address = IPAddress.Any;
                switch (_uri.HostNameType)
                {
                    case UriHostNameType.Dns:
                        address = Dns.GetHostAddresses(_uri.DnsSafeHost).First();
                        break;
                    case UriHostNameType.IPv4:
                    case UriHostNameType.IPv6:
                        address = IPAddress.Parse(_uri.Host);
                        break;
                    default:
                        break;
                }

                _listener = new TcpListener(address, _uri.Port);
                _listener.Start();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void StopListen()
        {
            try
            {
                _listener.Stop();
            }
            finally
            {
                _listener = null;
            }
        }

        public override Uri Uri
        {
            get
            {
                return _uri;
            }
        }

        protected override void OnAbort()
        {
            _listener.Stop();
            _listener = null;
        }

        private CustomTcpSocketSessionChannel newChannel(Socket socket)
        {
            var channel = new CustomTcpSocketSessionChannel(socket, this, _factory, _context);
            channel.Opened += Channel_Opened;
            channel.Closed += Channel_Closed;
            return channel;
        }

        private void Channel_Closed(object sender, EventArgs e)
        {
            if (_stub == null)
                return;
            _stub.OnClose(sender as ISocketChannel);
        }

        private void Channel_Opened(object sender, EventArgs e)
        {
            if (_stub == null)
                return;
            _stub.OnOpen(sender as ISocketChannel);
        }

        protected override IDuplexSessionChannel OnAcceptChannel(TimeSpan timeout)
        {
            return newChannel(_listener.AcceptSocket());
        }

        protected override IAsyncResult OnBeginAcceptChannel(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return _listener.BeginAcceptSocket(callback, state);
        }

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            StopListen();
            return new CompletedAsyncResult(callback, state);
        }

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            OnOpen(timeout);
            return new CompletedAsyncResult(callback, state);
        }

        protected override IAsyncResult OnBeginWaitForChannel(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new CompletedAsyncResult(callback, state);
        }

        protected override void OnClose(TimeSpan timeout)
        {
            StopListen();
        }

        protected override IDuplexSessionChannel OnEndAcceptChannel(IAsyncResult result)
        {
            return newChannel(_listener.EndAcceptSocket(result));
        }

        protected override void OnEndClose(IAsyncResult result)
        {
            CompletedAsyncResult.End(result);
        }

        protected override void OnEndOpen(IAsyncResult result)
        {
            CompletedAsyncResult.End(result);
        }

        protected override bool OnEndWaitForChannel(IAsyncResult result)
        {
            CompletedAsyncResult.End(result);
            return _listener.Pending();
        }

        protected override void OnOpen(TimeSpan timeout)
        {
            StartListen();
        }

        protected override bool OnWaitForChannel(TimeSpan timeout)
        {
            return _listener.Pending();
        }
    }
}
