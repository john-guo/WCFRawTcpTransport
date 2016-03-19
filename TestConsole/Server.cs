﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WCFRawTcpTransport;

namespace TestConsole
{
    class Server : WCFTcpListener
    {
        public Server(string uri) : base(uri, new SimpleProtocolEncoder())
        {

        }

        protected override void OnData(string sessionId, byte[] data)
        {
            Console.WriteLine(BitConverter.ToString(data));

            GC.Collect();

            Callback(data);
        }

        protected override void OnDisconnect(ISocketChannel session)
        {
            Console.WriteLine("Disconnect");
        }

        protected override void OnConnect(ISocketChannel session)
        {
            Console.WriteLine("Connect");
        }
    }
}
