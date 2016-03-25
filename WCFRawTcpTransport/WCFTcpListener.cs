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

        public WCFTcpListener(string uri, bool needEncoder, IRealEncoder encoder) : base(needEncoder, encoder)
        {
            _sessions = new ConcurrentDictionary<string, SessionItem>();

            _host = new ServiceHost(_stub);

            var endpoint = _host.AddServiceEndpoint(typeof(IInvokerService), _customBinding, uri);
            var behavior = new InvokerServiceEndpointBehavior();
            endpoint.EndpointBehaviors.Add(behavior);
        }
        
        public WCFTcpListener(string uri, bool needEncoder) : this(uri, needEncoder, null)
        {

        }

        public WCFTcpListener(string uri) : this(uri, true, null)
        {

        }

        protected virtual SessionItem GetSessionItem(string sessionId)
        {
            SessionItem item;
            if (!_sessions.TryGetValue(sessionId, out item))
                return null;

            return item;
        }

        protected virtual void Callback(string sessionId, byte[] data)
        {
            try
            {
                var item = GetSessionItem(sessionId);
                if (item == null)
                    return;

                if (item.Callback == null)
                    return;

                item.Callback.Invoke(sessionId, data);
            }
            catch
            { }
        }

        protected virtual void Callback(byte[] data)
        {
            try
            {
                _currentCallback.Invoke(string.Empty, data);
            }
            catch { }
        }

        protected virtual void Close(string sessionId)
        {
            var item = GetSessionItem(sessionId);
            item.Session.Close();
        }

        protected override void OnInvoke(string sessionId, byte[] data)
        {
            var item = GetSessionItem(sessionId);

            if (item == null)
                return;

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
            foreach (var sessionId in _sessions.Keys)
            {
                Close(sessionId);
            }

            _host.Close(TimeSpan.Zero);
        }
    }
}
