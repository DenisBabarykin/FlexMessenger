﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SocketsWrapper
{
    public interface INetClient
    {
        byte[] Recieve();
        void Send(byte[] byteAr);
        void Close();
        Socket Client { get; set; }
    }
}
