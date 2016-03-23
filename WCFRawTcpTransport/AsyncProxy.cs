using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;

namespace WCFRawTcpTransport
{
    interface IAsyncProxyChannel : ISocketChannel
    {
        IPoolManager<AsyncProxy> PoolManager { get; }
        ConcurrentQueue<ArraySegment<byte>> ReceviedBufferQueue { get; }
    }

    class AsyncProxy
    {
        public object State { get; set; }
        public AsyncCallback Callback { get; set; }
        public byte[] Buffer { get; set; }
        public int Count { get; set; }
        public int Offset { get; set; }
        public IAsyncProxyChannel Channel { get; set; }

        public void Clear()
        {
            State = null;
            Callback = null;
            Buffer = null;
            Count = 0;
            Offset = 0;
            Channel = null;
        }

        public void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                int size = Channel.Socket.EndReceive(ar);

                Channel.ReceviedBufferQueue.Enqueue(new ArraySegment<byte>(Buffer, 0, size));

                Callback(ar);
            }
            catch
            {
                Channel.Close();
            }
            finally
            {
                var poolMgr = Channel.PoolManager.Pool;
                Clear();
                poolMgr.Return(this);
            }
        }

        public void SendCallback(IAsyncResult ar)
        {
            try
            {
                int size = Channel.Socket.EndSend(ar);

                if (size < Count)
                {
                    Offset += size;
                    Count -= size;

                    Channel.Socket.BeginSend(Buffer, Offset, Count, SocketFlags.None, SendCallback, State);
                }
                else
                {
                    Channel.PoolManager.Buffer.ReturnBuffer(Buffer);
                    Callback(ar);
                }
            }
            catch
            {
                Channel.Close();
            }
            finally
            {
                var poolMgr = Channel.PoolManager.Pool;
                Clear();
                poolMgr.Return(this);
            }

        }
    }
}
