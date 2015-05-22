using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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
            int incomingLength;
            byte[] recievedMessage;

            using (var br = new BinaryReader(tcpClient.GetStream()))
            {
                incomingLength = br.ReadInt32();
                recievedMessage = br.ReadBytes(incomingLength);
            }
            return recievedMessage;
        }

        public void Send(byte[] byteAr)
        {
            using (var binWriter = new BinaryWriter(netStream))
            {
                binWriter.Write(byteAr.Length);
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
