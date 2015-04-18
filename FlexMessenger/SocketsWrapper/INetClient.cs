using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketsWrapper
{
    public interface INetClient
    {
        public byte[] Recieve();
        public void Send(byte[] byteAr);
        public void Close();
    }
}
