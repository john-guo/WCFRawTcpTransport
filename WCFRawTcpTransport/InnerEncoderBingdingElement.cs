using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;

namespace WCFRawTcpTransport
{
    public class InnerEncoderBingdingElement : BindingElement
    {
        private IRealEncoder _encoder;

        public override BindingElement Clone()
        {
            return new InnerEncoderBingdingElement(_encoder);
        }

        public InnerEncoderBingdingElement(IRealEncoder encoder)
        {
            _encoder = encoder;
        }

        public override T GetProperty<T>(BindingContext context)
        {
            if (typeof(T) == typeof(IRealEncoder))
                return (T)_encoder;

            return context.GetInnerProperty<T>();
        }

        public IRealEncoder Encoder
        {
            get { return _encoder; }
        }
    }
}
