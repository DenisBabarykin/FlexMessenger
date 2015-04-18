using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SocketsWrapper
{
    public interface INetListener
    {
        public void Start();
        public void Stop();
        public EndPoint LocalEndpoint { get; }
        public NetClient AcceptClient();
    }
}
