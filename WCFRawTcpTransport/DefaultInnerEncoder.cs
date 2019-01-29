using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;

namespace WCFRawTcpTransport
{
    class DefaultInnerEncoder : IRealEncoder
    {
        public bool TryRead(ISegmentBuffer buffer, BufferManager bufferManager, out ArraySegment<byte> data)
        {
            var count = buffer.Count;
            var item = bufferManager.TakeBuffer(count);
            if (!buffer.TryTake(count, item))
            {
                data = new ArraySegment<byte>();
                return false;
            }

            data = new ArraySegment<byte>(item, 0, count);
            return true;
        }

        public ArraySegment<byte> TryWrite(byte[] data, BufferManager bufferManager, int messageOffset)
        {
            return new ArraySegment<byte>(data, messageOffset, data.Length);
        }
    }
}
