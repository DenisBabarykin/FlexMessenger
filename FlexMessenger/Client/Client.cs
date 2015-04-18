using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Configuration;
using SocketsWrapper;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Formatters.Soap;
using FileLoggerLib;
using Encryption;

namespace FlexMessengerClient
{
    public class Client
    {
        Thread recieverThread;     
        bool _conn = false;    
        bool _logged = false;  
        string _user;        
        string _pass;          
        bool reg;

        IFormatter serializator;

        Message curMessage;

        public string Server { get { return ConfigurationManager.AppSettings["ServiceIP"]; } }  
        public int Port { get { return Convert.ToInt32(ConfigurationManager.AppSettings["Port"]); } }

        static string logFileName = Environment.CurrentDirectory + "\\" + ConfigurationManager.AppSettings["logFileName"];  //          

        static string lType = ConfigurationManager.AppSettings["loggerType"];   //

        static LoggerType logType = lType == "Server" ? LoggerType.SERVER : LoggerType.CLIENT;    //

        public Object thisLock = new Object();     //

        public FileLogger fileLogger = new FileLogger(logFileName, logType);   //

        SymmetricAlgorithm sa;
        AsymmetricAlgorithm asa;

        byte[] SAKey;
        byte[] SAIV; 


        public bool IsLoggedIn { get { return _logged; } }
        public string UserName { get { return _user; } }
        public string Password { get { return _pass; } }

        void connect(string user, string password, bool register)
        {
            if (!_conn)
            {
                _conn = true;
                _user = user;
                _pass = password;
                reg = register;
                recieverThread = new Thread(new ThreadStart(SetupConnection));
                recieverThread.Start();
            }
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
                return (Message)serializator.Deserialize(stream);
            }
        }

        public void Login(string user, string password)
        {
            connect(user, password, false);
        }
        public void Register(string user, string password)
        {
            connect(user, password, true);
        }
        public void Disconnect()
        {
            if (_conn)
                CloseConnection();
        }

        public void IsAvailable(string user)
        {
            if (_conn)
            {
                curMessage = new Message() { type = MessageType.IsAvailable, recipient = user };
                netClient.Send(Encrypt(Serialize(curMessage)));
            }
        }
        public void SendMessage(string to, string msg)
        {
            if (_conn)
            {
                curMessage = new Message() { type = MessageType.Send, recipient = to, message = msg };
                netClient.Send(Encrypt(Serialize(curMessage)));
            }
        }

        public event EventHandler LoginOK;
        public event EventHandler RegisterOK;
        public event FMErrorEventHandler LoginFailed;
        public event FMErrorEventHandler RegisterFailed;
        public event EventHandler Disconnected;
        public event AvailEventHandler UserAvailable;
        public event ReceivedEventHandler MessageReceived;
        
        virtual protected void OnLoginOK()
        {
            if (LoginOK != null)
            {
                LoginOK(this, EventArgs.Empty);
                lock (thisLock)
                {
                    fileLogger.WriteLogFile("LoginOK: \n"+ _user);
                }
            }
        }
        virtual protected void OnRegisterOK()
        {
            if (RegisterOK != null)
            {
                RegisterOK(this, EventArgs.Empty);
                lock (thisLock)
                {
                    fileLogger.WriteLogFile("RegisterOK: \n" + _user);
                }
            }
        }
        virtual protected void OnLoginFailed(FMErrorEventArgs e)
        {
            if (LoginFailed != null)
            {
                LoginFailed(this, e);
                lock (thisLock)
                {
                    fileLogger.WriteLogFile("LoginFailed: \n" + _user);
                }
            }
        }
        virtual protected void OnRegisterFailed(FMErrorEventArgs e)
        {
            if (RegisterFailed != null)
            {
                RegisterFailed(this, e);
                lock (thisLock)
                {
                    fileLogger.WriteLogFile("RegisterFailed: \n" + _user);
                }
            }
        }
        virtual protected void OnDisconnected()
        {
            if (Disconnected != null)
            {
                Disconnected(this, EventArgs.Empty);
                lock (thisLock)
                {
                    fileLogger.WriteLogFile("Disconnected: \n" + _user);
                    fileLogger.CloseLogFile();
                }
            }

        }
        virtual protected void OnUserAvail(AvailEventArgs e)
        {
            if (UserAvailable != null)
                UserAvailable(this, e);
        }
        virtual protected void OnMessageReceived(ReceivedEventArgs e)
        {
            if (MessageReceived != null)
                MessageReceived(this, e);
        }

        NetClient netClient;

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

        void SetupConnection() 
        {
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
            netClient = new NetClient(Server, Port);

            asa.getPrepared();
            byte[] RSAEncrypt = asa.getParametrs(false);

            curMessage = new Message() { type = MessageType.RSAPublicRequest, data = RSAEncrypt};
            netClient.Send(Serialize(curMessage));

            RSAEncrypt = asa.getParametrs(true);
            SAKey = asa.Decrypt(Deserialize(netClient.Recieve()).data, RSAEncrypt);
            SAIV = asa.Decrypt(Deserialize(netClient.Recieve()).data, RSAEncrypt);

            curMessage = Deserialize(Decrypt(netClient.Recieve()));
            if (curMessage.type == MessageType.Hello)
            {
                curMessage = new Message() { type = MessageType.Hello };
                netClient.Send(Encrypt(Serialize(curMessage)));
                curMessage = new Message() { type = reg ? MessageType.Register : MessageType.Login, message = UserName };
                netClient.Send(Encrypt(Serialize(curMessage)));
                curMessage = new Message() { type = reg ? MessageType.Register : MessageType.Login, message = Password };
                netClient.Send(Encrypt(Serialize(curMessage)));

                curMessage = Deserialize(Decrypt(netClient.Recieve()));
                if (curMessage.type == MessageType.OK) 
                {
                    if (reg)
                        OnRegisterOK(); 
                    OnLoginOK();  
                    Receiver(); 
                }
                else
                {
                    FMErrorEventArgs err = new FMErrorEventArgs((FMError)curMessage.type);
                    if (reg)
                        OnRegisterFailed(err);
                    else
                        OnLoginFailed(err);
                }
            }
            if (_conn)
                CloseConnection();
        }
        void CloseConnection() 
        {
            netClient.Close();
            OnDisconnected();
            _conn = false;
        }
        void Receiver()  
        {
            _logged = true;

            try
            {
                while (netClient.Client.Connected)  
                {
                    curMessage = Deserialize(Decrypt(netClient.Recieve()));

                    if (curMessage.type == MessageType.IsAvailable)
                    {
                        string user = curMessage.recipient;
                        bool isAvail = (curMessage.message == "true") ? true : false;
                        OnUserAvail(new AvailEventArgs(user, isAvail));
                    }
                    else if (curMessage.type == MessageType.Received)
                    {
                        string from = curMessage.sender;
                        string msg = curMessage.message;
                        OnMessageReceived(new ReceivedEventArgs(from, msg));
                    }
                }
            }
            catch (IOException) { }

            _logged = false;
        }
    }
}
