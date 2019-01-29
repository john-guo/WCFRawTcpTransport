using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;

namespace WCFRawTcpTransport
{
    public class SocketSession : ISession
    {
        private Socket _socket;
        public SocketSession(Socket socket)
        {
            _socket = socket;
        }

        public string Id => _socket.Handle.ToString();
    }
}
