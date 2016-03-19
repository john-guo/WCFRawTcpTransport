using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;

namespace WCFRawTcpTransport
{
    interface IPoolManager<T> where T : class, new()
    {
        BufferManager Buffer { get; }
        SimpleObjectPool<T> Pool { get; }
    }
}
