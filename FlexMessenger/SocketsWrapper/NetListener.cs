using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SocketsWrapper
{
    public class NetListener : INetListener
    {
        TcpListener tcpListener;
        public void Start()
        {
            throw new NotImplementedException();
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }

        public System.Net.EndPoint LocalEndpoint
        {
            get { throw new NotImplementedException(); }
        }

        public NetClient AcceptClient()
        {
            throw new NotImplementedException();
        }
    }
}
