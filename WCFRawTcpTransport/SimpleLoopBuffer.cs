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
    class SimpleLoopBuffer : ISegementBuffer
    {
        private byte[] buffer;
        private int begin;
        private int end;
        private int capacity;
        private ReaderWriterLockSlim locker;

        public SimpleLoopBuffer(int maxSize)
        {
            capacity = maxSize;
            buffer = new byte[maxSize + 1];
            locker = new ReaderWriterLockSlim();
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
            locker.EnterWriteLock();
            begin = end = 0;
            locker.ExitWriteLock();
        }

        public bool TryAdd(byte[] data)
        {
            return TryAdd(data, 0, data.Length);
        }

        public bool TryAdd(byte[] data, int offset, int length)
        {
            bool ret = false;

            locker.EnterWriteLock();

            do
            {
                if (length + offset > data.Length)
                    break;

                if (length > Size - Count)
                    break;

                if (begin > end)
                {
                    Buffer.BlockCopy(data, offset, buffer, end, length);
                    end += length;
                    ret = true;
                    break;
                }

                int remain = buffer.Length - end;
                if (length <= remain)
                {
                    Buffer.BlockCopy(data, offset, buffer, end, length);
                    end = (end + length) % buffer.Length;
                    ret = true;
                    break;
                }

                Buffer.BlockCopy(data, offset, buffer, end, remain);
                Buffer.BlockCopy(data, remain, buffer, 0, length - remain);

                end = length - remain;

                ret = true;

            } while (false);

            locker.ExitWriteLock();

            return ret;
        }

        public void Skip(int count)
        {
            locker.EnterWriteLock();
            if (!IsEmpty)
                begin = (begin + count) % buffer.Length;
            locker.ExitWriteLock();
        }

        public bool TryTake(int count, byte[] data)
        {
            bool ret = false;

            locker.EnterReadLock();

            do
            {
                if (count > Count)
                    break;

                if (count == 0)
                    break;

                if (end >= begin)
                {
                    Buffer.BlockCopy(buffer, begin, data, 0, count);
                    begin += count;
                    ret = true;
                    break;
                }

                int remain = buffer.Length - begin;
                if (count <= remain)
                {
                    Buffer.BlockCopy(buffer, begin, data, 0, count);
                    begin = (begin + count) % buffer.Length;
                    ret = true;
                    break;
                }

                Buffer.BlockCopy(buffer, begin, data, 0, remain);
                Buffer.BlockCopy(buffer, 0, data, remain, count - remain);

                begin = count - remain;

                ret = true;
            } while (false);

            locker.ExitReadLock();

            return ret;
        }

        public bool TryPeek(int count, byte[] data)
        {
            bool ret = false;

            locker.EnterReadLock();

            do
            {
                if (count > Count)
                    break;

                if (count == 0)
                    break;

                if (end >= begin)
                {
                    Buffer.BlockCopy(buffer, begin, data, 0, count);
                    ret = true;
                    break;
                }

                int remain = buffer.Length - begin;
                if (count <= remain)
                {
                    Buffer.BlockCopy(buffer, begin, data, 0, count);
                    ret = true;
                    break;
                }

                Buffer.BlockCopy(buffer, begin, data, 0, remain);
                Buffer.BlockCopy(buffer, 0, data, remain, count - remain);
                ret = true;

            } while (false);

            locker.ExitReadLock();

            return ret;
        }

    }
}
