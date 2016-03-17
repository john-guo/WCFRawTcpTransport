using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace WCFRawTcpTransport
{
    public delegate void InvokeDelegate(string sessionId, byte[] data);
    public delegate void ConnectionEventDelegate(ISocketChannel session);

    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, 
        UseSynchronizationContext = false,
        InstanceContextMode = InstanceContextMode.Single)]
    public class InvokerStub : IInvokerService, IInvokerServiceCallback
    {
        public event InvokeDelegate OnInvoke = delegate { };
        public event ConnectionEventDelegate Connect = delegate { };
        public event ConnectionEventDelegate Disconnect = delegate { };

        public virtual void Invoke(string sessionId, byte[] data)
        {
            OnInvoke(sessionId, data);
        }

        internal virtual void OnOpen(ISocketChannel session)
        {
            Connect(session);
        }

        internal virtual void OnClose(ISocketChannel session)
        {
            Disconnect(session);
        }
    }
}
