using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Text;
using System.Threading.Tasks;

namespace WCFRawTcpTransport
{
    public abstract class WCFTcpClient : WCFTcpBase, IDisposable
    {
        private DuplexChannelFactory<IInvokerService> _factory;
        private IInvokerService _service;

        public WCFTcpClient(string uri, IRealEncoder encoder)
        {
            var customBinding = new CustomBinding();
            if (encoder != null)
                customBinding.Elements.Add(new InnerEncoderBingdingElement(encoder));
            customBinding.Elements.Add(new CustomEncodingBindingElement());
            customBinding.Elements.Add(new CustomTcpBindingElement(_stub));

            var address = new EndpointAddress(uri);
            var endpoint = new ServiceEndpoint(ContractDescription.GetContract(typeof(IInvokerService)), customBinding, address);
            var behavior = new InvokerServiceEndpointBehavior();
            endpoint.EndpointBehaviors.Add(behavior);

            _factory = new DuplexChannelFactory<IInvokerService>(new InstanceContext(_stub), endpoint);
            _service = _factory.CreateChannel();
        }

        public WCFTcpClient(string uri) : this(uri, null)
        {

        }

        public IInvokerService Service
        {
            get { return _service; }
        }

        public void Invoke(byte[] data)
        {
            Service.Invoke(string.Empty, data);
        }

        public override void Dispose()
        {
            _factory.Close();
        }
    }
}
