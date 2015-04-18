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
using Encryption;

namespace Service
{
    public class ClientHandler
    {
        INetClient netClient;
        UserInfo userInfo;
        IFormatter serializator;
        Message curMessage;

        SymmetricAlgorithm sa;
        AsymmetricAlgorithm asa;

        byte[] SAKey;
        byte[] SAIV; 

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

            switch (ConfigurationManager.AppSettings["SA"])
            {
                case "TripleDesSymmetricAlgorithm":
                    sa = new TripleDesSymmetricAlgorithm();
                    break;
                case "AesSymmetricAlgorithm":
                    sa = new AesSymmetricAlgorithm();
                    break;
                default:
                    sa = new TripleDesSymmetricAlgorithm();
                    break;
            }

            switch (ConfigurationManager.AppSettings["ASA"])
            {
                case "RSAAsymmetricAlgorithm":
                    asa = new RSAAsymmetricAlgorithm();
                    break;
                default:
                    asa = new RSAAsymmetricAlgorithm();
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

        public byte[] Encrypt(byte[] data)
        {
            byte[] res = sa.Encrypt(data, SAKey, SAIV);
            return res;
        }

        public byte[] Decrypt(byte[] data)
        {
            byte[] res = sa.Decrypt(data, SAKey, SAIV);
            return res;
        }


        void SetupConnection()  // Setup connection and login or register.
        {
            try
            {
                Console.WriteLine("[{0}] New connection!", DateTime.Now);

                string date = DateTime.Now.ToString();
                lock (FacadeSingleton.Instance.thisLock)
                {
                    FacadeSingleton.Instance.fileLogger.WriteLogFile(date + " New connection!" + "\n");
                }

                curMessage = Deserialize(netClient.Recieve());
                byte[] RSAEncrypt = curMessage.data;

                sa.getPrepared();
                SAKey = sa.getKey();
                SAIV = sa.getIV();

                byte[] encrypted = asa.Encrypt(SAKey, RSAEncrypt);
                curMessage = new Message { type = MessageType.SymmetricKey, data = encrypted };
                netClient.Send(Serialize(curMessage));

                encrypted = asa.Encrypt(SAIV, RSAEncrypt);
                curMessage = new Message { type = MessageType.SymmetricIV, data = encrypted };
                netClient.Send(Serialize(curMessage));

                curMessage = new Message { type = MessageType.Hello };
                netClient.Send(Encrypt(Serialize(curMessage)));

                curMessage = Deserialize(Decrypt(netClient.Recieve()));
                if (curMessage.type == MessageType.Hello)
                {
                    curMessage = Deserialize(Decrypt(netClient.Recieve()));
                    int logMode = (int) curMessage.type;
                    string userName = Deserialize(Decrypt(netClient.Recieve())).message;
                    string password = Deserialize(Decrypt(netClient.Recieve())).message;
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
                                    netClient.Send(Encrypt(Serialize(curMessage)));
                                    Console.WriteLine("[{0}] ({1}) Registered new user", DateTime.Now, userName);

                                    lock (FacadeSingleton.Instance.thisLock)
                                    {
                                        FacadeSingleton.Instance.fileLogger.WriteLogFile(date + " Registered new user " + userName + "\n");
                                    }


                                    FacadeSingleton.Instance.SaveUsers();
                                    Receiver();
                                }
                                else
                                {
                                    curMessage = new Message { type = MessageType.Exists };
                                    netClient.Send(Encrypt(Serialize(curMessage)));
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
                                        netClient.Send(Encrypt(Serialize(curMessage)));
                                        Receiver();  // Listen to client in loop.
                                    }
                                    else
                                    {
                                        curMessage = new Message { type = MessageType.WrongPass };
                                        netClient.Send(Encrypt(Serialize(curMessage)));
                                    }
                                }
                                else
                                {
                                    curMessage = new Message { type = MessageType.NoExists };
                                    netClient.Send(Encrypt(Serialize(curMessage)));
                                }
                            }
                        }
                        else
                        {
                            curMessage = new Message { type = MessageType.TooPassword };
                            netClient.Send(Encrypt(Serialize(curMessage)));
                        }
                    }
                    else
                    {
                        curMessage = new Message { type = MessageType.TooUsername };
                        netClient.Send(Encrypt(Serialize(curMessage)));
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

                string date = DateTime.Now.ToString();
                lock (FacadeSingleton.Instance.thisLock)
                {
                    FacadeSingleton.Instance.fileLogger.WriteLogFile(date + " End of connection!" + "\n");
                }


            }
            catch { }
        }
        void Receiver()
        {
            Console.WriteLine("[{0}] ({1}) User logged in", DateTime.Now, userInfo.UserName);

            string date = DateTime.Now.ToString();
            lock (FacadeSingleton.Instance.thisLock)
            {
                FacadeSingleton.Instance.fileLogger.WriteLogFile(date + " User logged in " + userInfo.UserName + "\n");
            }


            userInfo.LoggedIn = true;
            Message curMessage;

            try
            {
                while (netClient.Client.Connected)
                {
                    curMessage = Deserialize(Decrypt(netClient.Recieve()));
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
                        netClient.Send(Encrypt(Serialize(curMessage)));
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
                                netClient.Send(Encrypt(Serialize(curMessage)));
                                Console.WriteLine("[{0}] ({1} -> {2}) Message sent!", DateTime.Now, userInfo.UserName, recipient.UserName);

                                lock (FacadeSingleton.Instance.thisLock)
                                {
                                    FacadeSingleton.Instance.fileLogger.WriteLogFile(date + " Message sent! " + userInfo.UserName + "  " + recipient.UserName + "\n");
                                }
                            }
                        }
                    }
                }
            }
            catch (IOException) { }

            userInfo.LoggedIn = false;
            Console.WriteLine("[{0}] ({1}) User logged out", DateTime.Now, userInfo.UserName);

            lock (FacadeSingleton.Instance.thisLock)
            {
                FacadeSingleton.Instance.fileLogger.WriteLogFile(date + " User logged out " + userInfo.UserName + "\n");
            }
        }
    }
}
