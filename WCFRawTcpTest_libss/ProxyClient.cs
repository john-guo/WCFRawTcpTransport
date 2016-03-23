using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WCFRawTcpTransport;

namespace WCFRawTcpTest_libss
{
    internal class ProxyClient : WCFTcpClient
    {
        private string sid;
        public event Action<string, byte[]> DataReceived = delegate { };
        public event Action<string> Closed = delegate { };


        public ProxyClient(string uri, string sessionId) : base(uri)
        {
            sid = sessionId;
        }

        protected override void OnConnect(ISocketChannel session)
        {
        }

        protected override void OnData(string sessionId, byte[] data)
        {
            DataReceived(SessionId, data);
        }

        protected override void OnDisconnect(ISocketChannel session)
        {
            Closed(SessionId);
        }

        public override void Invoke(byte[] data)
        {
            base.Invoke(data);
        }

        public string SessionId
        {
            get { return sid; }
        }
    }
}
