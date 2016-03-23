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
    public abstract class WCFTcpClient : WCFTcpBase
    {
        private DuplexChannelFactory<IInvokerService> _factory;
        private IInvokerService _service;

        public WCFTcpClient(string uri, IRealEncoder encoder) : base(encoder)
        {
            var address = new EndpointAddress(uri);
            var endpoint = new ServiceEndpoint(ContractDescription.GetContract(typeof(IInvokerService)), _customBinding, address);
            var behavior = new InvokerServiceEndpointBehavior();
            endpoint.EndpointBehaviors.Add(behavior);

            _factory = new DuplexChannelFactory<IInvokerService>(new InstanceContext(_stub), endpoint);
        }

        public WCFTcpClient(string uri) : this(uri, null)
        {

        }

        public IInvokerService Service
        {
            get { return _service; }
        }

        public virtual void Invoke(byte[] data)
        {
            Service.Invoke(string.Empty, data);
        }

        public override void Open()
        {
            _service = _factory.CreateChannel();
            (_service as IChannel).Open();
        }

        public override void Close()
        {
            _factory.Close();
        }
    }
}
