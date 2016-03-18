﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;

namespace WCFRawTcpTransport
{
    class CustomTcpChannelFactory : ChannelFactoryBase<IDuplexChannel>
    {
        private TcpClient _client;
        private MessageEncoderFactory _factory;
        private BindingContext _context;
        private InvokerStub _stub;

        internal CustomTcpChannelFactory(BindingContext context, MessageEncoderFactory factory, InvokerStub stub)
        {
            _context = context;
            _factory = factory;
            _stub = stub;
        }

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new CompletedAsyncResult(callback, state);
        }

        protected override IDuplexChannel OnCreateChannel(EndpointAddress address, Uri via)
        {
            _client = new TcpClient(address.Uri.Host, address.Uri.Port);
            var channel = new CustomTcpSocketChannel(_client.Client, this, _factory, _context);
            channel.Opened += Channel_Opened;
            channel.Closed += Channel_Closed;

            return channel;
        }

        private void Channel_Closed(object sender, EventArgs e)
        {
            if (_stub == null)
                return;
            _stub.OnClose(sender as ISocketChannel);
        }

        private void Channel_Opened(object sender, EventArgs e)
        {
            if (_stub == null)
                return;
            _stub.OnOpen(sender as ISocketChannel);
        }

        protected override void OnEndOpen(IAsyncResult result)
        {
            CompletedAsyncResult.End(result);
        }

        protected override void OnOpen(TimeSpan timeout)
        {
        }
    }
}