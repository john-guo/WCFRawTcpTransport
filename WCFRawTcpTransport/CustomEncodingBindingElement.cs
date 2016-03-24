using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;

namespace WCFRawTcpTransport
{
    public class CustomEncodingBindingElement
        : MessageEncodingBindingElement
    {
        private InnerEncoderBingdingElement _innerElement;
        private MessageVersion _version = MessageVersion.Default;

        public override MessageVersion MessageVersion
        {
            get
            {
                return _version;
            }

            set
            {
                _version = value;
            }
        }

        public override BindingElement Clone()
        {
            return new CustomEncodingBindingElement();
        }

        public override MessageEncoderFactory CreateMessageEncoderFactory()
        {
            return new CustomEncoderFactory(this, _innerElement);
        }

        private void SetInnerElement(BindingContext context)
        {
            if (_innerElement != null)
                return;

            _innerElement = context.Binding.Elements.OfType<InnerEncoderBingdingElement>().FirstOrDefault();
        }

        public override bool CanBuildChannelFactory<TChannel>(BindingContext context)
        {
            SetInnerElement(context);
            context.BindingParameters.Add(this);
            return context.CanBuildInnerChannelFactory<TChannel>();
        }

        public override bool CanBuildChannelListener<TChannel>(BindingContext context)
        {
            SetInnerElement(context);
            context.BindingParameters.Add(this);
            return context.CanBuildInnerChannelListener<TChannel>();
        }

        public override IChannelFactory<TChannel> BuildChannelFactory<TChannel>(BindingContext context)
        {
            SetInnerElement(context);
            context.BindingParameters.Add(this);
            return context.BuildInnerChannelFactory<TChannel>();
        }

        public override IChannelListener<TChannel> BuildChannelListener<TChannel>(BindingContext context)
        {
            SetInnerElement(context);
            context.BindingParameters.Add(this);
            return context.BuildInnerChannelListener<TChannel>();
        }
    }
}
