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
        public override BindingElement Clone()
        {
            return new InnerEncoderBingdingElement(Encoder);
        }

        public InnerEncoderBingdingElement(IRealEncoder encoder)
        {
            Encoder = encoder;
        }

        public override T GetProperty<T>(BindingContext context)
        {
            if (typeof(T) == typeof(IRealEncoder))
                return (T)Encoder;

            return context.GetInnerProperty<T>();
        }

        public IRealEncoder Encoder { get; }
    }
}
