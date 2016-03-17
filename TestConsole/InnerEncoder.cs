﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;
using WCFRawTcpTransport;

namespace TestConsole
{
    class InnerEncoder : IRealEncoder
    {
        public bool TryRead(ISegementBuffer buffer, BufferManager bufferManager, out ArraySegment<byte> data)
        {
            byte[] item;
            data = default(ArraySegment<byte>);

            if (buffer.Count < CustomTransportConstant.minMessageSize)
                return false;

            int header = 0;
            do
            {
                item = bufferManager.TakeBuffer(CustomTransportConstant.headerSize);
                if (!buffer.TryPeek(CustomTransportConstant.headerSize, item))
                {
                    bufferManager.ReturnBuffer(item);
                    return false;
                }

                header = BitConverter.ToInt32(item, 0);
                header = IPAddress.NetworkToHostOrder(header);
                bufferManager.ReturnBuffer(item);

                if (header > buffer.Count - CustomTransportConstant.headerSize)
                    return false;

                buffer.Skip(CustomTransportConstant.headerSize);

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
            var packetLen = data.Length + CustomTransportConstant.headerSize;
            var body = bufferManager.TakeBuffer(packetLen);
            int header = IPAddress.HostToNetworkOrder(data.Length);
            var hb = BitConverter.GetBytes(header);
            Buffer.BlockCopy(hb, 0, body, messageOffset, hb.Length);
            Buffer.BlockCopy(data, 0, body, messageOffset + hb.Length, data.Length);

            return new ArraySegment<byte>(body, messageOffset, packetLen);
        }
    }
}
