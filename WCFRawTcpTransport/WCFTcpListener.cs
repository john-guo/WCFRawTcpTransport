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
    public abstract class WCFTcpListener : WCFTcpBase, IDisposable
    {
        private ServiceHost _host;
        private IInvokerServiceCallback _currentCallback;
        private ConcurrentDictionary<string, SessionItem> _sessions;

        class SessionItem
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

            _host.Open();
        }

        public WCFTcpListener(string uri) : this(uri, null)
        {

        }

        protected IInvokerServiceCallback Callback
        {
            get
            {
                return _currentCallback;
            }
        }

        protected override void OnInvoke(string sessionId, byte[] data)
        {
            SessionItem item;
            _sessions.TryGetValue(sessionId, out item);

            //BUG: only one client (first connected) can be called back.
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

        public override void Dispose()
        {
            _host.Close();
        }
    }
}
