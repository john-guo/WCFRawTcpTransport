using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WCFRawTcpTransport
{
    //TODO Optimize and thread safe
    class SimpleLoopBuffer : ISegementBuffer
    {
        private byte[] buffer;
        private int begin;
        private int end;
        private int capacity;

        public SimpleLoopBuffer(int maxSize)
        {
            capacity = maxSize;
            buffer = new byte[maxSize + 1];
            Clear();
        }

        public bool IsEmpty
        {
            get { return begin == end; }
        }
        
        public bool IsFull
        {
            get
            {
                if (begin == 0 && end == capacity)
                {
                    return true;
                }

                return begin - 1 == end;
            }
        }

        public int Begin
        {
            get { return begin; }
        }

        public int End
        {
            get { return end; }
        }

        public int Size
        {
            get { return capacity; }
        }

        public int Count
        {
            get
            {
                if (begin > end)
                {
                    return (Size - begin) + end; 
                }

                return end - begin;
            }
        }

        public byte[] Array
        {
            get { return buffer; }
        }

        public void Clear()
        {
            begin = end = 0;
        }

        public bool TryAdd(byte[] data)
        {
            return TryAdd(data, 0, data.Length);
        }

        public bool TryAdd(byte[] data, int offset, int length)
        {
            if (length + offset > data.Length)
                return false;

            if (length > Size - Count)
                return false;

            if (begin > end)
            {
                Buffer.BlockCopy(data, offset, buffer, end, length);
                end += length;
                return true;
            }

            int remain = buffer.Length - end;
            if (length <= remain)
            {
                Buffer.BlockCopy(data, offset, buffer, end, length);
                end = (end + length) % buffer.Length;
                return true;
            }

            Buffer.BlockCopy(data, offset, buffer, end, remain);
            Buffer.BlockCopy(data, remain, buffer, 0, length - remain);

            end = length - remain;

            return true;          
        }

        public void Skip(int count)
        {
            begin = (begin + count) % buffer.Length;
        }

        public bool TryTake(int count, byte[] data)
        {
            if (count > Count)
                return false;

            if (count == 0)
                return false;

            if (end >= begin)
            {
                Buffer.BlockCopy(buffer, begin, data, 0, count);
                begin += count;
                return true;
            }
            
            int remain = buffer.Length - begin;
            if (count <= remain)
            {
                Buffer.BlockCopy(buffer, begin, data, 0, count);
                begin = (begin + count) % buffer.Length;
                return true;
            }

            Buffer.BlockCopy(buffer, begin, data, 0, remain);
            Buffer.BlockCopy(buffer, 0, data, remain, count - remain);

            begin = count - remain;

            return true;
        }

        public bool TryPeek(int count, byte[] data)
        {
            if (count > Count)
                return false;

            if (count == 0)
                return false;

            if (end >= begin)
            {
                Buffer.BlockCopy(buffer, begin, data, 0, count);
                return true;
            }

            int remain = buffer.Length - begin;
            if (count <= remain)
            {
                Buffer.BlockCopy(buffer, begin, data, 0, count);
                return true;
            }

            Buffer.BlockCopy(buffer, begin, data, 0, remain);
            Buffer.BlockCopy(buffer, 0, data, remain, count - remain);
            return true;
        }
    }
}
