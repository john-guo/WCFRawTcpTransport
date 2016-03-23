using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WCFRawTcpTransport;
using Shadowsocks.Encryption;
using System.Collections.Concurrent;

namespace WCFRawTcpTest_libss
{
    public class ProxyServer : WCFTcpListener
    {
        enum HandShakeState { First, Second, Final }
        private ConcurrentDictionary<string, ProxyClient> proxies;
        private ConcurrentDictionary<string, HandShakeState> states;
        private ConcurrentDictionary<string, byte[]> extraHeaders;
        private IEncryptor encryptor;
        private string remote;
        private const int BufferSize = CustomTransportConstant.MaxBufferSize * 2;
        private byte[] encryptBuffer = new byte[BufferSize];
        private byte[] decryptBuffer = new byte[BufferSize];


        private static string MakeUri(string address, int port)
        {
            return string.Format("{0}://{1}:{2}", CustomTransportConstant.Schema, address, port);
        }

        public ProxyServer(string localAddress, int localPort,
            string remoteAddress, int remotePort,
            string method, string password)
            :
            this(MakeUri(localAddress, localPort),
                  MakeUri(remoteAddress, remotePort),
                  method, password)
        { }

        public ProxyServer(string localUri, string remoteUri, string method, string password) : base(localUri)
        {
            remote = remoteUri;
            proxies = new ConcurrentDictionary<string, ProxyClient>();
            encryptor = EncryptorFactory.GetEncryptor(method, password);
            states = new ConcurrentDictionary<string, HandShakeState>();
            extraHeaders = new ConcurrentDictionary<string, byte[]>();
        }

        protected override void OnConnect(ISocketChannel session)
        {
            states[session.SessionId] = HandShakeState.First;
        }

        protected override void OnData(string sessionId, byte[] data)
        {
            HandShakeState state;
            if (!states.TryGetValue(sessionId, out state))
                throw new InvalidOperationException();

            switch (state)
            {
                case HandShakeState.First:
                    states[sessionId] = HandShakeState.Second;
                    Handshake(sessionId, data);
                    break;

                case HandShakeState.Second:
                    states[sessionId] = HandShakeState.Final;
                    HandshakeSecond(sessionId, data);
                    break;

                case HandShakeState.Final:
                    Pass(sessionId, data);
                    break;
            }
        }

        protected override void OnDisconnect(ISocketChannel session)
        {
            CloseSession(session.SessionId, false, true);
        }

        private void Handshake(string sessionId, byte[] data)
        {
            if (data.Length <= 1)
            {
                Close();
                return;
            }

            byte[] response = { 5, 0 };
            if (data[0] != 5)
            {
                // reject socks 4
                response = new byte[] { 0, 91 };
            }

            Callback(sessionId, response);
        }

        private ProxyClient NewProxy(string sessionId)
        {
            var client = new ProxyClient(remote, sessionId);
            client.DataReceived += Proxy_DataReceived;
            client.Closed += Proxy_Closed;
            proxies.TryAdd(sessionId, client);
            return client;
        }

        private ProxyClient GetProxy(string sessionId)
        {
            ProxyClient proxy = null;
            proxies.TryGetValue(sessionId, out proxy);
            return proxy;
        }

        private void HandshakeSecond(string sessionId, byte[] data)
        {
            if (data.Length < 3)
            {
                Close();
                return;
            }
            if (data.Length > 3)
            {
                var extraHeader = new byte[data.Length - 3];
                Array.Copy(data, 3, extraHeader, 0, data.Length - 3);
                extraHeaders[sessionId] = extraHeader;
            }            

            var command = data[1];
            if (command == 1)
            {
                var proxy = NewProxy(sessionId);
                proxy.Open();

                byte[] response = { 5, 0, 0, 1, 0, 0, 0, 0, 0, 0 };
                Callback(response);
            }
            else if (command == 3)
            {
                throw new NotSupportedException();
            }
        }

        private void Pass(string sessionId, byte[] data)
        {
            int size;
            byte[] buffer1, buffer2;

            lock (encryptBuffer)
            {
                buffer2 = data;
                byte[] extraHeader;
                if (extraHeaders.TryRemove(sessionId, out extraHeader))
                {
                    buffer2 = new byte[extraHeader.Length + data.Length];
                    Array.Copy(extraHeader, buffer2, extraHeader.Length);
                    Array.Copy(data, 0, buffer2, extraHeader.Length, data.Length);
                }
                encryptor.Encrypt(buffer2, buffer2.Length, encryptBuffer, out size);
                buffer1 = Truncate(encryptBuffer, size);
            }

            var proxy = GetProxy(sessionId);
            proxy.Invoke(buffer1);
        }

        private byte[] Truncate(byte[] array, int size)
        {
            var data = new byte[size];

            Array.Copy(array, data, size);
            return data;
        }

        private void CloseSession(string sessionId, bool needCloseLocal = true, bool needCloseRemote = false)
        {
            ProxyClient proxy = null;
            if (proxies.TryRemove(sessionId, out proxy))
            {
                if (needCloseRemote)
                    proxy.Close();
            }

            HandShakeState state;
            states.TryRemove(sessionId, out state);

            byte[] extraHead;
            extraHeaders.TryRemove(sessionId, out extraHead);

            if (!needCloseLocal)
                return;

            var item = GetSessionItem(sessionId);
            if (item == null)
                return;
            item.Session.Close();
        }


        private void Proxy_DataReceived(string sessionId, byte[] data)
        {
            int size;
            byte[] buffer;

            lock (decryptBuffer)
            {
                encryptor.Decrypt(data, data.Length, decryptBuffer, out size);
                buffer = Truncate(decryptBuffer, size);
            }

            Callback(sessionId, buffer);
        }

        private void Proxy_Closed(string sessionId)
        {
            CloseSession(sessionId);            
        }

        public override void Close()
        {
            base.Close();
            encryptor.Dispose();
        }
    }
}
