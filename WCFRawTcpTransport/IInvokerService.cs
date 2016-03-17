using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Channels;
using System.Threading;
using System.Collections.Concurrent;
using System.IO;

namespace WCFRawTcpTransport
{
    [ServiceContract(CallbackContract = typeof(IInvokerServiceCallback), SessionMode = SessionMode.NotAllowed)]
    public interface IInvokerService 
    {
        [OperationContract(IsOneWay = true, Action = CustomTransportConstant.Action)]
        [InvokerBehavior]
        void Invoke(string sessionId, byte[] data);
    }

    public interface IInvokerServiceCallback
    {
        [OperationContract(IsOneWay = true, Action = CustomTransportConstant.Action)]
        [InvokerBehavior]
        void Invoke(string sessionId, byte[] data);
    }
}
