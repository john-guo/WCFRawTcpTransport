using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace WCFRawTcpTransport
{
    public class CustomEncoder : MessageEncoder
    {
        private MessageEncoderFactory _encodeFactory;
        private IRealEncoder _encoder;
        private SimpleLoopBuffer _receiveBuffer;

         public CustomEncoder(MessageEncoderFactory factory, IRealEncoder encoder)
        {
            _encodeFactory = factory;
            _encoder = encoder;
            _receiveBuffer = new SimpleLoopBuffer(CustomTransportConstant.MaxBufferSize * CustomTransportConstant.MaxBufferPoolSize);
        }

        public virtual IRealEncoder InnerEncoder
        {
            get
            {
                return _encoder;
            }
        }

        public override string ContentType
        {
            get
            {
                return CustomTransportConstant.BinaryMIME;
            }
        }

        public override string MediaType
        {
            get
            {
                return CustomTransportConstant.BinaryMIME;
            }
        }

        public override MessageVersion MessageVersion
        {
            get
            {
                return _encodeFactory.MessageVersion;
            }
        }

        private Message ReadMessageWithoutInnerEncoder(ArraySegment<byte> buffer, BufferManager bufferManager)
        {
            if (buffer.Count == 0)
                return null;

            byte[] obj = new byte[buffer.Count];
            Buffer.BlockCopy(buffer.Array, buffer.Offset, obj, 0, buffer.Count);
            var message = Message.CreateMessage(MessageVersion, CustomTransportConstant.Action, obj);
            bufferManager.ReturnBuffer(buffer.Array);

            return message;
        }

        public ArraySegment<byte> WriteMessageWithoutInnerEncoder(byte[] data, int messageOffset)
        {
            return new ArraySegment<byte>(data, messageOffset, data.Length);
        }

        public override Message ReadMessage(ArraySegment<byte> buffer, BufferManager bufferManager, string contentType)
        {
            if (InnerEncoder == null)
            {
                return ReadMessageWithoutInnerEncoder(buffer, bufferManager);
            }

            if (buffer.Count > 0)
            {
                var result = _receiveBuffer.TryAdd(buffer.Array, buffer.Offset, buffer.Count);
                if (!result)
                    throw new OutOfMemoryException();
                bufferManager.ReturnBuffer(buffer.Array);
            }

            if (_receiveBuffer.Count == 0)
                return null;

            ArraySegment<byte> data;
            if (!InnerEncoder.TryRead(_receiveBuffer, bufferManager, out data))
                return null;

            byte[] obj = new byte[data.Count];
            Buffer.BlockCopy(data.Array, data.Offset, obj, 0, data.Count);
            var message = Message.CreateMessage(MessageVersion, CustomTransportConstant.Action, obj);
            bufferManager.ReturnBuffer(data.Array);

            return message;
        }

        public override Message ReadMessage(Stream stream, int maxSizeOfHeaders, string contentType)
        {
            throw new NotSupportedException();
        }

        public override void WriteMessage(Message message, Stream stream)
        {
            throw new NotSupportedException();
        }

        public override ArraySegment<byte> WriteMessage(Message message, int maxMessageSize, BufferManager bufferManager, int messageOffset)
        {
            var reader = message.GetReaderAtBodyContents();
            var data = reader.ReadElementContentAsBase64();

            if (InnerEncoder == null)
            {
                return WriteMessageWithoutInnerEncoder(data, messageOffset);
            }

            return InnerEncoder.TryWrite(data, bufferManager, messageOffset);
        }
    }
}
