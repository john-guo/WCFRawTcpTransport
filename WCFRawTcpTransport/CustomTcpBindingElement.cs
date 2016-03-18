using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;

namespace WCFRawTcpTransport
{
    public class CustomTcpBindingElement :
        TransportBindingElement
    {
        private InvokerStub _stub;

        internal CustomTcpBindingElement() : this(null)
        {
        }

        internal CustomTcpBindingElement(InvokerStub stub) : base()
        {
            UseSession = true;
            _stub = stub;
        }

        public override string Scheme
        {
            get
            {
                return CustomTransportConstant.Schema;
            }
        }

        public bool UseSession { get; set; }

        public override BindingElement Clone()
        {
            return new CustomTcpBindingElement(_stub);
        }

        private MessageEncoderFactory getMessageEncoderFactory(BindingContext context)
        {
            var element = context.BindingParameters.OfType<MessageEncodingBindingElement>().FirstOrDefault();
            return element.CreateMessageEncoderFactory();
        }

        public override IChannelFactory<TChannel> BuildChannelFactory<TChannel>(BindingContext context)
        {
            var factory = getMessageEncoderFactory(context);
            if (UseSession)
                return (IChannelFactory<TChannel>)new CustomTcpSessionChannelFactory(context, factory, _stub);

            return (IChannelFactory<TChannel>)new CustomTcpChannelFactory(context, factory, _stub);
        }

        public override IChannelListener<TChannel> BuildChannelListener<TChannel>(BindingContext context)
        {
            var factory = getMessageEncoderFactory(context);
            if (UseSession)
                return (IChannelListener<TChannel>)new CustomTcpSessionChannelListener(context, factory, _stub);

            return (IChannelListener<TChannel>)new CustomTcpChannelListener(context, factory, _stub);
        }

        public override bool CanBuildChannelFactory<TChannel>(BindingContext context)
        {
            if (UseSession)
                return (typeof(TChannel) == typeof(IDuplexSessionChannel));

            return (typeof(TChannel) == typeof(IDuplexChannel));
        }

        public override bool CanBuildChannelListener<TChannel>(BindingContext context)
        {
            if (UseSession)
                return (typeof(TChannel) == typeof(IDuplexSessionChannel));

            return (typeof(TChannel) == typeof(IDuplexChannel));
        }
    }
}
