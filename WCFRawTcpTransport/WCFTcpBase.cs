using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WCFRawTcpTransport
{
    public abstract class WCFTcpBase : IDisposable
    {
        protected InvokerStub _stub;

        protected WCFTcpBase()
        {
            _stub = new InvokerStub();
            _stub.OnInvoke += OnInvoke;
            _stub.Connect += _OnConnect;
            _stub.Disconnect += _OnDisconnect;
        }

        protected virtual void OnInvoke(string sessionId, byte[] data)
        {
            OnData(sessionId, data);
        }

        protected virtual void _OnDisconnect(ISocketChannel session)
        {
            OnDisconnect(session);
        }

        protected virtual void _OnConnect(ISocketChannel session)
        {
            OnConnect(session);
        }

        protected abstract void OnDisconnect(ISocketChannel session);
        protected abstract void OnConnect(ISocketChannel session);
        protected abstract void OnData(string sessionId, byte[] data);

        public virtual void Dispose()
        {
        }
    }
}
