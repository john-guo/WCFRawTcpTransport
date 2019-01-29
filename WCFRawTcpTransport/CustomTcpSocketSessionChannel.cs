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
    class CustomTcpSocketSessionChannel : CustomTcpSocketChannel, IDuplexSessionChannel
    {
        internal CustomTcpSocketSessionChannel(Socket socket, ChannelManagerBase channelManager, MessageEncoderFactory factory, BindingContext context)
            : base(socket, channelManager, factory, context)
        {
            _session = new SocketDuplexSession(socket);
        }

        private SocketDuplexSession _session;

        public IDuplexSession Session
        {
            get
            {
                return _session;
            }
        }
     }
}
