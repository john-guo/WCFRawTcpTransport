using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WCFRawTcpTransport
{
    public interface ISegmentBuffer
    {
        int Begin { get; }
        int End { get; }
        int Count { get; }

        void Clear();
        byte[] Array { get; }
        bool IsFull { get; }
        bool IsEmpty { get; }

        bool TryAdd(byte[] data, int offset, int length);
        bool TryPeek(int count, byte[] data);
        bool TryTake(int count, byte[] data);
        void Skip(int count);
    }
}
