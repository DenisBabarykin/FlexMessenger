using SocketsWrapper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Formatters.Soap;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Service
{
    public class ClientHandler
    {
        INetClient netClient;
        UserInfo userInfo;
        IFormatter serializator;

        public ClientHandler(INetClient netClient) 
        {
            this.netClient = netClient;
            switch (ConfigurationManager.AppSettings["Serialization"])
            {
                case "Binary":
                    serializator = new BinaryFormatter();
                    break;
                case "SOAP":
                    serializator = new SoapFormatter();
                    break;
                default:
                    serializator = new BinaryFormatter();
                    break;
            }
            (new Thread(new ThreadStart(SetupConnection))).Start();
        }

        public byte[] Serialize(Message message)
        {
            using (var stream = new MemoryStream())
            {
                serializator.Serialize(stream, message);
                return stream.ToArray();
            }
        }

        public Message Deserialize(byte[] binaryData)
        {
            using (var stream = new MemoryStream(binaryData))
            {
                return (Message) serializator.Deserialize(stream);
            }
        }

        void SetupConnection()  // Setup connection and login or register.
        {
            try
            {
                Console.WriteLine("[{0}] New connection!", DateTime.Now);
                Message curMessage = new Message { type = MessageType.Hello };
                netClient.Send(Serialize(curMessage));

                curMessage = Deserialize(netClient.Recieve());
                if (curMessage.type == MessageType.Hello)
                {
                    curMessage = Deserialize(netClient.Recieve());
                    int logMode = (int) curMessage.type;
                    string userName = Deserialize(netClient.Recieve()).message;
                    string password = Deserialize(netClient.Recieve()).message;
                    if (userName.Length < 10) 
                    {
                        if (password.Length < 20)
                        {
                            if (logMode == (int) MessageType.Register)
                            {
                                if (!FacadeSingleton.Instance.users.ContainsKey(userName))
                                {
                                    userInfo = new UserInfo(userName, password, this);
                                    FacadeSingleton.Instance.users.Add(userName, userInfo);
                                    curMessage = new Message { type = MessageType.OK };
                                    netClient.Send(Serialize(curMessage));
                                    Console.WriteLine("[{0}] ({1}) Registered new user", DateTime.Now, userName);
                                    FacadeSingleton.Instance.SaveUsers();
                                    Receiver();
                                }
                                else
                                {
                                    curMessage = new Message { type = MessageType.Exists };
                                    netClient.Send(Serialize(curMessage));
                                }
                            }
                            else if (logMode == (int) MessageType.Login) 
                            {
                                if (FacadeSingleton.Instance.users.TryGetValue(userName, out userInfo))
                                {
                                    if (password == userInfo.Password)
                                    {
                                        if (userInfo.LoggedIn)
                                            userInfo.Connection.CloseConnection();

                                        userInfo.Connection = this;
                                        curMessage = new Message { type = MessageType.OK };
                                        netClient.Send(Serialize(curMessage));
                                        Receiver();  // Listen to client in loop.
                                    }
                                    else
                                    {
                                        curMessage = new Message { type = MessageType.WrongPass };
                                        netClient.Send(Serialize(curMessage));
                                    }
                                }
                                else
                                {
                                    curMessage = new Message { type = MessageType.NoExists };
                                    netClient.Send(Serialize(curMessage));
                                }
                            }
                        }
                        else
                        {
                            curMessage = new Message { type = MessageType.TooPassword };
                            netClient.Send(Serialize(curMessage));
                        }
                    }
                    else
                    {
                        curMessage = new Message { type = MessageType.TooUsername };
                        netClient.Send(Serialize(curMessage));
                    }
                }
                CloseConnection();
            }
            catch { CloseConnection(); }
        }
        void CloseConnection()
        {
            try
            {
                userInfo.LoggedIn = false;
                netClient.Close();
                Console.WriteLine("[{0}] End of connection!", DateTime.Now);
            }
            catch { }
        }
        void Receiver()
        {
            Console.WriteLine("[{0}] ({1}) User logged in", DateTime.Now, userInfo.UserName);
            userInfo.LoggedIn = true;
            Message curMessage;

            try
            {
                while (netClient.Client.Connected)
                {
                    curMessage = Deserialize(netClient.Recieve());
                    if (curMessage.type == MessageType.IsAvailable)
                    {
                        string who = curMessage.recipient;
                        UserInfo info;
                        if (FacadeSingleton.Instance.users.TryGetValue(who, out info))
                        {
                            if (info.LoggedIn)
                                curMessage = new Message { type = MessageType.IsAvailable, recipient = who, message = "true" };
                            else
                                curMessage = new Message { type = MessageType.IsAvailable, recipient = who, message = "false" };
                        }
                        else
                            curMessage = new Message { type = MessageType.IsAvailable, recipient = who, message = "false" };
                        netClient.Send(Serialize(curMessage));
                    }
                    else if (curMessage.type == MessageType.Send)
                    {
                        string to = curMessage.recipient;
                        string msg = curMessage.message;

                        UserInfo recipient;
                        if (FacadeSingleton.Instance.users.TryGetValue(to, out recipient))
                        {
                            if (recipient.LoggedIn)
                            {
                                curMessage = new Message { type = MessageType.Received, sender = userInfo.UserName, message = msg };
                                netClient.Send(Serialize(curMessage));
                                Console.WriteLine("[{0}] ({1} -> {2}) Message sent!", DateTime.Now, userInfo.UserName, recipient.UserName);
                            }
                        }
                    }
                }
            }
            catch (IOException) { }

            userInfo.LoggedIn = false;
            Console.WriteLine("[{0}] ({1}) User logged out", DateTime.Now, userInfo.UserName);
        }
    }
}
