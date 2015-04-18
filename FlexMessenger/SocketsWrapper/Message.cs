using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketsWrapper
{
    public enum MessageType
    {
        Hello,
        OK,
        Login,
        Register,
        TooUsername,
        TooPassword,
        Exists,
        NoExists,
        WrongPass,
        IsAvailable,
        Send,
        Received,
        RSAPublicRequest,
        SymmetricKey,
        SymmetricIV
    }

    [Serializable]
    public class Message
    {
        public MessageType type;
        public string recipient;
        public string sender;
        public string message;
        public byte[] data;
    }
}
