using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace WCFRawTcpTransport
{
    public abstract class WCFTcpListener : WCFTcpBase
    {
        private ServiceHost _host;
        private IInvokerServiceCallback _currentCallback;
        private ConcurrentDictionary<string, SessionItem> _sessions;

        protected class SessionItem
        {
            public ISocketChannel Session;
            public IInvokerServiceCallback Callback;
        }

        public WCFTcpListener(string uri, IRealEncoder encoder) : base(encoder)
        {
            _sessions = new ConcurrentDictionary<string, SessionItem>();

            _host = new ServiceHost(_stub);

            var endpoint = _host.AddServiceEndpoint(typeof(IInvokerService), _customBinding, uri);
            var behavior = new InvokerServiceEndpointBehavior();
            endpoint.EndpointBehaviors.Add(behavior);
        }

        public WCFTcpListener(string uri) : this(uri, null)
        {

        }

        protected SessionItem GetSessionItem(string sessionId)
        {
            SessionItem item;
            if (!_sessions.TryGetValue(sessionId, out item))
                throw new InvalidOperationException();

            return item;
        }

        protected void Callback(string sessionId, byte[] data)
        {
            var item = GetSessionItem(sessionId);
            if (item.Callback == null)
                throw new InvalidOperationException();

            item.Callback.Invoke(sessionId, data);
        }

        protected void Callback(byte[] data)
        {
            _currentCallback.Invoke(string.Empty, data);
        }

        protected override void OnInvoke(string sessionId, byte[] data)
        {
            var item = GetSessionItem(sessionId);

            if (item.Callback == null)
                item.Callback = OperationContext.Current.GetCallbackChannel<IInvokerServiceCallback>();
            _currentCallback = item.Callback;
            
            OnData(sessionId, data);
        }

        protected override void _OnConnect(ISocketChannel session)
        {
            _sessions[session.SessionId] = new SessionItem() { Session = session, Callback = null };

            base._OnConnect(session);
        }

        protected override void _OnDisconnect(ISocketChannel session)
        {
            base._OnDisconnect(session);

            SessionItem item;

            _sessions.TryRemove(session.SessionId, out item);
        }

        public override void Open()
        {
            _host.Open();
        }

        public override void Close()
        {
            _host.Close();
        }
    }
}
