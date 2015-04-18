using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SocketsWrapper
{
    public class NetListener : INetListener
    {
        TcpListener tcpListener;

        public NetListener(IPAddress localaddr, int port)
        {
            tcpListener = new TcpListener(localaddr, port);
        }
        public void Start()
        {
            tcpListener.Start();
        }

        public void Stop()
        {
            tcpListener.Stop();
        }

        public System.Net.EndPoint LocalEndpoint
        {
            get { return tcpListener.LocalEndpoint; }
        }

        public NetClient AcceptClient()
        {
            return new NetClient(tcpListener.AcceptTcpClient());
        }
    }
}
