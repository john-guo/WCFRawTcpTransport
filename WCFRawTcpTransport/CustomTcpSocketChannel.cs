using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Collections.Concurrent;
using System.Xml;

namespace WCFRawTcpTransport
{
    class CustomTcpSocketChannel : ChannelBase, IDuplexChannel, ISocketChannel, IAsyncProxyChannel
    {
        private Socket _socket;
        private MessageEncoderFactory _factory;
        private EndpointAddress _local;
        private EndpointAddress _remote;
        private BindingContext _context;
        private ConcurrentQueue<Message> _msgQueue;
        private MessageEncoder _encoder;
        private ConcurrentQueue<ArraySegment<byte>> _queue;

        internal CustomTcpSocketChannel(Socket socket, ChannelManagerBase channelManager, MessageEncoderFactory factory, BindingContext context)
            : base(channelManager)
        {
            if (PoolManager == null)
                throw new NotSupportedException();

            _socket = socket;
            _socket.LingerState = new LingerOption(false, 0);
            _socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, true);
            _socket.SendTimeout = Convert.ToInt32(DefaultSendTimeout.TotalMilliseconds);
            _socket.ReceiveTimeout = Convert.ToInt32(DefaultReceiveTimeout.TotalMilliseconds);

            _factory = factory;
            _context = context;
            _encoder = factory.Encoder;

            _local = getEndpointAddress(_socket.LocalEndPoint);
            _remote = getEndpointAddress(_socket.RemoteEndPoint);

            _queue = new ConcurrentQueue<ArraySegment<byte>>();
            _msgQueue = new ConcurrentQueue<Message>();
        }

        public ConcurrentQueue<ArraySegment<byte>> ReceviedBufferQueue
        {
            get { return _queue; }
        }

        public IPoolManager<AsyncProxy> PoolManager
        {
            get { return Manager as IPoolManager<AsyncProxy>; }
        }

        private EndpointAddress getEndpointAddress(EndPoint ep)
        {
            var iep = ep as IPEndPoint;

            var builder = new UriBuilder();
            builder.Scheme = _context.Binding.Scheme;
            builder.Host = iep.Address.ToString();
            builder.Port = iep.Port;

            return new EndpointAddress(builder.Uri);
        }

        public EndpointAddress LocalAddress
        {
            get
            {
                return _local;
            }
        }

        public EndpointAddress RemoteAddress
        {
            get
            {
                return _remote;
            }
        }

        public Uri Via
        {
            get
            {
                return null;
            }
        }

        public Socket Socket
        {
            get
            {
                return _socket;
            }
        }

        public bool AllowOutputBatching
        {
            get; set;
        }

        public IInputSession InputSession
        {
            get
            {
                return null;
            }
        }

        public TimeSpan OperationTimeout
        {
            get; set;
        }

        public IOutputSession OutputSession
        {
            get
            {
                return null;
            }
        }

        public string SessionId
        {
            get
            {
                return _socket.Handle.ToString();

            }
        }

        public IExtensionCollection<IContextChannel> Extensions
        {
            get
            {
                return null;
            }
        }



        private Message AssemblyMessage(Message message)
        {
            message.Headers.Add(MessageHeader.CreateHeader(CustomTransportConstant.SessionMessageHeader, CustomTransportConstant.SessionNamespace, SessionId));
            return message;
        }

        private Message DisassemblyMessage(Message message)
        {
            return message;
        }

        public IAsyncResult BeginReceive(AsyncCallback callback, object state)
        {
            return BeginReceive(TimeSpan.MinValue, callback, state);
        }

        public IAsyncResult BeginReceive(TimeSpan timeout, AsyncCallback callback, object state)
        {
            var buffer = PoolManager.Buffer.TakeBuffer(CustomTransportConstant.MaxBufferSize);

            var asyncItem = PoolManager.Pool.Take();
            asyncItem.Channel = this;
            asyncItem.Buffer = buffer;
            asyncItem.State = state;
            asyncItem.Callback = callback;

            try
            {
                return _socket.BeginReceive(buffer, 0, CustomTransportConstant.MaxBufferSize, SocketFlags.None, asyncItem.ReceiveCallback, state);
            }
            catch
            {
                Close();
                return null;
            }
        }

        public IAsyncResult BeginSend(Message message, AsyncCallback callback, object state)
        {
            return BeginSend(message, TimeSpan.MinValue, callback, state);
        }

        public IAsyncResult BeginSend(Message message, TimeSpan timeout, AsyncCallback callback, object state)
        {
            message = DisassemblyMessage(message);
            var buffer = _encoder.WriteMessage(message, int.MaxValue, PoolManager.Buffer, 0);
            int offset = buffer.Offset;
            int count = buffer.Count;

            var asyncItem = PoolManager.Pool.Take();
            asyncItem.Channel = this;
            asyncItem.Buffer = buffer.Array;
            asyncItem.State = state;
            asyncItem.Callback = callback;
            asyncItem.Offset = offset;
            asyncItem.Count = count;

            try
            {
                return _socket.BeginSend(buffer.Array, offset, count, SocketFlags.None, asyncItem.SendCallback, state);
            }
            catch
            {
                Close();
                return null;
            }
        }

        public IAsyncResult BeginTryReceive(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return BeginReceive(timeout, callback, state);
        }

        public IAsyncResult BeginWaitForMessage(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return BeginTryReceive(timeout, callback, state);
        }

        public Message EndReceive(IAsyncResult result)
        {
            Message message;
            EndTryReceive(result, out message);
            return message;
        }

        public void EndSend(IAsyncResult result)
        {
            try
            {
                _socket.EndSend(result);
            }
            catch
            {
                Close();
            }
        }

        public bool EndTryReceive(IAsyncResult result, out Message message)
        {
            if (_msgQueue.TryDequeue(out message))
            {
                return true;
            }

            ArraySegment<byte> item;

            _queue.TryDequeue(out item);

            message = _encoder.ReadMessage(item, PoolManager.Buffer);

            if (message == null)
                return false;

            message = AssemblyMessage(message);

            return true;
        }

        public bool EndWaitForMessage(IAsyncResult result)
        {
            Message message;
            bool ret = EndTryReceive(result, out message);
            if (!ret)
                return false;
            _msgQueue.Enqueue(message);
            return true;
        }

        public Message Receive()
        {
            return Receive(TimeSpan.MinValue);
        }

        public Message Receive(TimeSpan timeout)
        {
            Message message;
            TryReceive(timeout, out message);
            return message; 
        }

        public void Send(Message message)
        {
            Send(message, TimeSpan.MinValue);
        }

        public void Send(Message message, TimeSpan timeout)
        {
            try
            {
                message = DisassemblyMessage(message);
                var data = _encoder.WriteMessage(message, int.MaxValue, PoolManager.Buffer);
                int count = data.Count;
                int offset = data.Offset;
                do
                {
                    int size = _socket.Send(data.Array, offset, count, SocketFlags.None);
                    count -= size;
                    offset += size;
                } while (count > 0);
            }
            catch
            {
                Close();
            }
        }

        public bool TryReceive(TimeSpan timeout, out Message message)
        {
            message = null;
            if (_msgQueue.TryDequeue(out message))
                return true;

            try
            {
                var buffer = PoolManager.Buffer.TakeBuffer(CustomTransportConstant.MaxBufferSize);
                int size = _socket.Receive(buffer);
                message = _encoder.ReadMessage(new ArraySegment<byte>(buffer, 0, size), PoolManager.Buffer);
            }
            catch
            {
                Close();
                return false;
            }

            if (message == null)
                return false;

            message = AssemblyMessage(message);

            return true;
        }

        public bool WaitForMessage(TimeSpan timeout)
        {
            return !_msgQueue.IsEmpty;
        }

        private void ForceClose()
        {
            try
            {
                _socket.Shutdown(SocketShutdown.Both);
                //Cannot invoke Disconnect here, this would crash current thread, reason was unknown.
                //_socket.Disconnect(true);
                _socket.Close();
            }
            catch
            { }
        }

        protected override void OnAbort()
        {
            ForceClose();
        }

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            OnClose(timeout);
            return new CompletedAsyncResult(callback, state);
        }

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new CompletedAsyncResult(callback, state);
        }

        protected override void OnClose(TimeSpan timeout)
        {
            ForceClose();
        }

        protected override void OnEndClose(IAsyncResult result)
        {
            CompletedAsyncResult.End(result);
        }

        protected override void OnEndOpen(IAsyncResult result)
        {
            CompletedAsyncResult.End(result);
        }

        protected override void OnOpen(TimeSpan timeout)
        {

        }

        public override T GetProperty<T>()
        {
            if (typeof(T) == typeof(ISocketChannel))
                return (T)(ISocketChannel)this;
            return base.GetProperty<T>();
        }
    }
}
