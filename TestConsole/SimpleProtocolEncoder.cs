using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;
using WCFRawTcpTransport;

namespace TestConsole
{
    class SimpleProtocolEncoder : IRealEncoder
    {
        const int headerSize = 4;
        const int minMessageSize = headerSize + 1;


        public bool TryRead(ISegementBuffer buffer, BufferManager bufferManager, out ArraySegment<byte> data)
        {
            byte[] item;
            data = default(ArraySegment<byte>);

            if (buffer.Count < minMessageSize)
                return false;

            int header = 0;
            do
            {
                item = bufferManager.TakeBuffer(headerSize);
                if (!buffer.TryPeek(headerSize, item))
                {
                    bufferManager.ReturnBuffer(item);
                    return false;
                }

                header = BitConverter.ToInt32(item, 0);
                header = IPAddress.NetworkToHostOrder(header);
                bufferManager.ReturnBuffer(item);

                if (header > buffer.Count - headerSize)
                    return false;

                buffer.Skip(headerSize);

            } while (header <= 0);

            item = bufferManager.TakeBuffer(header);
            if (!buffer.TryTake(header, item))
            {
                throw new OutOfMemoryException();
            }

            data = new ArraySegment<byte>(item, 0, header);
            return true;
        }

        public ArraySegment<byte> TryWrite(byte[] data, BufferManager bufferManager, int messageOffset)
        {
            var packetLen = data.Length + headerSize;
            var body = bufferManager.TakeBuffer(packetLen);
            int header = IPAddress.HostToNetworkOrder(data.Length);
            var hb = BitConverter.GetBytes(header);
            Buffer.BlockCopy(hb, 0, body, messageOffset, hb.Length);
            Buffer.BlockCopy(data, 0, body, messageOffset + hb.Length, data.Length);

            return new ArraySegment<byte>(body, messageOffset, packetLen);
        }
    }
}
