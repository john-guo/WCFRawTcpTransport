using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Text;
using System.Threading.Tasks;

namespace WCFRawTcpTransport
{
    public class InvokerBehaviorAttribute : Attribute, IOperationBehavior, IClientMessageFormatter, IDispatchMessageFormatter
    {
        public void AddBindingParameters(OperationDescription operationDescription, BindingParameterCollection bindingParameters)
        {

        }

        public void ApplyClientBehavior(OperationDescription operationDescription, ClientOperation clientOperation)
        {
            clientOperation.Formatter = this;
            clientOperation.SerializeRequest = true;
            clientOperation.DeserializeReply = true;
        }

        public void ApplyDispatchBehavior(OperationDescription operationDescription, DispatchOperation dispatchOperation)
        {
            dispatchOperation.Formatter = this;
            dispatchOperation.SerializeReply = true;
            dispatchOperation.DeserializeRequest = true;
        }

        public object DeserializeReply(Message message, object[] parameters)
        {
            DeserializeRequest(message, parameters);
            return null;
        }

        public void DeserializeRequest(Message message, object[] parameters)
        {
            var index = message.Headers.FindHeader(CustomTransportConstant.SessionMessageHeader, CustomTransportConstant.SessionNamespace);
            string sessionId = string.Empty;
            if (index >= 0)
                sessionId = message.Headers.GetHeader<string>(index);

            parameters[0] = sessionId;
            parameters[1] = message.GetReaderAtBodyContents().ReadElementContentAsBase64();
        }

        public Message SerializeReply(MessageVersion messageVersion, object[] parameters, object result)
        {
            return SerializeRequest(messageVersion, parameters);
        }

        public Message SerializeRequest(MessageVersion messageVersion, object[] parameters)
        {
            return Message.CreateMessage(messageVersion, CustomTransportConstant.Action, parameters[1]);
        }

        public void Validate(OperationDescription operationDescription)
        {

        }
    }
}
