using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WCFRawTcpTransport
{
    public class CustomTransportConstant
    {
        public const int MaxBufferPoolSize = 100;
        public const int MaxBufferSize = 65536;
        public const int headerSize = 4;
        public const int minMessageSize = headerSize + 1;
        public const string Action = "__Invoker";
        public const string BinaryMIME = "application/octet-stream";
        public const string Schema = "tcp";
        public const string SessionMessageHeader = "Session";
        public const string SessionNamespace = "";
    }
}
