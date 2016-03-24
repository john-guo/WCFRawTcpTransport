using Shadowsocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WCFRawTcpTest_sswinform.sites
{
    [Serializable]
    public class Server
    {
        public string server;
        public int server_port;
        public string password;
        public string method;
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class SSAttribute : Attribute
    {
    }

    public interface ISSSite
    {
        IList<Server> GetServers();
    }
}
