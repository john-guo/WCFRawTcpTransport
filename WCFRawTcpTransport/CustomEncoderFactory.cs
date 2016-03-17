using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;

namespace WCFRawTcpTransport
{
    public class CustomEncoderFactory
        : MessageEncoderFactory
    {
        private MessageEncoder _encoder;
        private MessageEncodingBindingElement _element;

        public CustomEncoderFactory(MessageEncodingBindingElement element, InnerEncoderBingdingElement innerElement)
        {
            _element = element;
            _encoder = new CustomEncoder(this, innerElement == null ? null : innerElement.Encoder);
        }

        public override MessageEncoder Encoder
        {
            get
            {
                return _encoder;
            }
        }

        public override MessageVersion MessageVersion
        {
            get
            {
                return _element.MessageVersion;
            }
        }
    }
}
