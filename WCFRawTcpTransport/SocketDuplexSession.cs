using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;

namespace WCFRawTcpTransport
{
    public class SocketDuplexSession : SocketSession, IDuplexSession
    {
        public SocketDuplexSession(Socket socket) : base(socket) { }

        public IAsyncResult BeginCloseOutputSession(AsyncCallback callback, object state)
        {
            return new CompletedAsyncResult(callback, state);
        }

        public IAsyncResult BeginCloseOutputSession(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new CompletedAsyncResult(callback, state);
        }

        public void CloseOutputSession()
        {
        }

        public void CloseOutputSession(TimeSpan timeout)
        {
        }

        public void EndCloseOutputSession(IAsyncResult result)
        {
            CompletedAsyncResult.End(result);
        }
    }
}
