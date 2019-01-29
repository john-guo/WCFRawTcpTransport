using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;

namespace WCFRawTcpTransport
{
    public interface IRealEncoder
    {
        bool TryRead(ISegmentBuffer buffer, BufferManager bufferManager, out ArraySegment<byte> data);
        ArraySegment<byte> TryWrite(byte[] data, BufferManager bufferManager, int messageOffset);
    }
}
