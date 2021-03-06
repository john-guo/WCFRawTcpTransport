﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;

namespace WCFRawTcpTransport
{
    public abstract class WCFTcpBase : IDisposable
    {
        protected InvokerStub _stub;
        protected CustomBinding _customBinding;

        protected WCFTcpBase(bool needEncoder, IRealEncoder encoder)
        {
            _stub = new InvokerStub();
            _stub.OnInvoke += OnInvoke;
            _stub.Connect += _OnConnect;
            _stub.Disconnect += _OnDisconnect;

            _customBinding = new CustomBinding();
            if (needEncoder && encoder == null)
                encoder = new DefaultInnerEncoder();
            if (encoder != null)
                _customBinding.Elements.Add(new InnerEncoderBingdingElement(encoder));
            _customBinding.Elements.Add(new CustomEncodingBindingElement());
            _customBinding.Elements.Add(new CustomTcpBindingElement(_stub));
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

        public abstract void Open();
        public abstract void Close();

        public virtual void Dispose()
        {
            Close();
        }
    }
}
