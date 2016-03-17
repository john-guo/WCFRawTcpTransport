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
        private IInvokerServiceCallback _callbackChannel;
        private ConcurrentDictionary<string, ISocketChannel> _sessions;

        public WCFTcpListener(string uri, IRealEncoder encoder)
        {
            _sessions = new ConcurrentDictionary<string, ISocketChannel>();

            _host = new ServiceHost(_stub);
            var customBinding = new CustomBinding();
            if (encoder != null)
                customBinding.Elements.Add(new InnerEncoderBingdingElement(encoder));
            customBinding.Elements.Add(new CustomEncodingBindingElement());
            customBinding.Elements.Add(new CustomTcpBindingElement(_stub));

            var endpoint = _host.AddServiceEndpoint(typeof(IInvokerService), customBinding, uri);
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
                return _callbackChannel;
            }
        }

        protected override void OnInvoke(string sessionId, byte[] data)
        {
            ISocketChannel session;
            _sessions.TryGetValue(sessionId, out session);

            //Cannot use the SocketChannel as ServiceChannel to retrive CallbackChannel.
            //using (var scope = new OperationContextScope(session))
            //{
            //    _callbackChannel = OperationContext.Current.GetCallbackChannel<IInvokerServiceCallback>();
            //    OnData(sessionId, data);
            //}


            //BUG: only one client (first connected) can be called back. 
            _callbackChannel = OperationContext.Current.GetCallbackChannel<IInvokerServiceCallback>();


            OnData(sessionId, data);
        }

        protected override void _OnConnect(ISocketChannel session)
        {
            _sessions[session.SessionId] = session;

            base._OnConnect(session);
        }

        protected override void _OnDisconnect(ISocketChannel session)
        {
            base._OnDisconnect(session);

            _sessions.TryRemove(session.SessionId, out session);
        }

        public override void Dispose()
        {
            _host.Close();
        }
    }
}
