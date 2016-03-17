using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace WCFRawTcpTransport
{
    class CustomBodyWriter : BodyWriter
    {
        string _sessionId;
        XmlDictionaryReader _reader;

        public CustomBodyWriter(string sessionId, XmlDictionaryReader reader) : base(true)
        {
            _sessionId = sessionId;
            _reader = reader;
        }

        protected override void OnWriteBodyContents(XmlDictionaryWriter writer)
        {
            writer.WriteString(_sessionId);
            while (!_reader.EOF)
                writer.WriteNode(_reader, false);
        }
    }
}
