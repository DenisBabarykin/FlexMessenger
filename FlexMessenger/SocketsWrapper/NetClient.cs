using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SocketsWrapper
{
    public class NetClient : INetClient
    {
        TcpClient tcpClient;
        NetworkStream netStream;

        public NetClient(TcpClient tcpClient)
        {
            this.tcpClient = tcpClient;
            netStream = tcpClient.GetStream();
        }
        public NetClient(string hostname, int port)
        {
            tcpClient = new TcpClient(hostname, port);
            netStream = tcpClient.GetStream();
        }
        public byte[] Recieve()
        {
            using (var ms = new MemoryStream())
            {
                netStream.CopyTo(ms);
                return ms.ToArray();
            }
        }

        public void Send(byte[] byteAr)
        {
            using (var binWriter = new BinaryWriter(netStream))
            {
                binWriter.Write(byteAr);
                binWriter.Close();
            }
        }

        public void Close()
        {
            tcpClient.Close();
        }

        public Socket Client { get { return tcpClient.Client; } set { tcpClient.Client = value; } }
    }
}
