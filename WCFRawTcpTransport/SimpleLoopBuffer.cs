using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WCFRawTcpTransport
{
    //TODO Optimize and thread safe
    class SimpleLoopBuffer : ISegmentBuffer
    {
        private readonly object locker;

        public SimpleLoopBuffer(int maxSize)
        {
            Size = maxSize;
            Array = new byte[maxSize + 1];
            locker = new object();
            Clear();
        }

        public bool IsEmpty
        {
            get { return Begin == End; }
        }
        
        public bool IsFull
        {
            get
            {
                if (Begin == 0 && End == Size)
                {
                    return true;
                }

                return Begin - 1 == End;
            }
        }

        public int Begin { get; private set; }

        public int End { get; private set; }

        public int Size { get; }

        public int Count
        {
            get
            {
                if (Begin > End)
                {
                    return (Size - Begin) + End; 
                }

                return End - Begin;
            }
        }

        public byte[] Array { get; }

        public void Clear()
        {
            lock(locker)
            {
                Begin = End = 0;

            }
        }

        public bool TryAdd(byte[] data)
        {
            return TryAdd(data, 0, data.Length);
        }

        public bool TryAdd(byte[] data, int offset, int length)
        {
            bool ret = false;

            lock (locker)
            {
                do
                {
                    if (length + offset > data.Length)
                        break;

                    if (length > Size - Count)
                        break;

                    if (Begin > End)
                    {
                        Buffer.BlockCopy(data, offset, Array, End, length);
                        End += length;
                        ret = true;
                        break;
                    }

                    int remain = Array.Length - End;
                    if (length <= remain)
                    {
                        Buffer.BlockCopy(data, offset, Array, End, length);
                        End = (End + length) % Array.Length;
                        ret = true;
                        break;
                    }

                    Buffer.BlockCopy(data, offset, Array, End, remain);
                    Buffer.BlockCopy(data, remain, Array, 0, length - remain);

                    End = length - remain;

                    ret = true;

                } while (false);

            }
            return ret;
        }

        public void Skip(int count)
        {
            lock (locker)
            {
                if (!IsEmpty)
                    Begin = (Begin + count) % Array.Length;
            }
        }

        public bool TryTake(int count, byte[] data)
        {
            bool ret = false;

            lock (locker)
            {

                do
                {
                    if (count > Count)
                        break;

                    if (count == 0)
                        break;

                    if (End >= Begin)
                    {
                        Buffer.BlockCopy(Array, Begin, data, 0, count);
                        Begin += count;
                        ret = true;
                        break;
                    }

                    int remain = Array.Length - Begin;
                    if (count <= remain)
                    {
                        Buffer.BlockCopy(Array, Begin, data, 0, count);
                        Begin = (Begin + count) % Array.Length;
                        ret = true;
                        break;
                    }

                    Buffer.BlockCopy(Array, Begin, data, 0, remain);
                    Buffer.BlockCopy(Array, 0, data, remain, count - remain);

                    Begin = count - remain;

                    ret = true;
                } while (false);

            }
            return ret;
        }

        public bool TryPeek(int count, byte[] data)
        {
            bool ret = false;

            lock (locker)
            {

                do
                {
                    if (count > Count)
                        break;

                    if (count == 0)
                        break;

                    if (End >= Begin)
                    {
                        Buffer.BlockCopy(Array, Begin, data, 0, count);
                        ret = true;
                        break;
                    }

                    int remain = Array.Length - Begin;
                    if (count <= remain)
                    {
                        Buffer.BlockCopy(Array, Begin, data, 0, count);
                        ret = true;
                        break;
                    }

                    Buffer.BlockCopy(Array, Begin, data, 0, remain);
                    Buffer.BlockCopy(Array, 0, data, remain, count - remain);
                    ret = true;

                } while (false);

            }
            return ret;
        }

    }
}
