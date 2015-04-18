using SocketsWrapper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using FileLoggerLib;

namespace Service
{
    public class Facade
    {
        string usersFilename = Environment.CurrentDirectory + "\\" + ConfigurationManager.AppSettings["UsersFilename"];

        static string logFileName = Environment.CurrentDirectory + "\\" + ConfigurationManager.AppSettings["logFileName"];  //          

        static string lType = ConfigurationManager.AppSettings["loggerType"];   //

        static LoggerType logType = lType == "Server" ? LoggerType.SERVER : LoggerType.CLIENT;    //

        public Object thisLock = new Object();     //

        public FileLogger fileLogger = new FileLogger(logFileName, logType);   //

        public Dictionary<string, UserInfo> users = new Dictionary<string, UserInfo>();

        INetListener netListener;
        
        public void Init()
        {
            Console.Title = "FlexMessenger Server";
            Console.WriteLine("----- FlexMessenger Server -----");

            lock (thisLock)
            {
                fileLogger.WriteLogFile("----- InstantMessenger Server -----\n");
            }

            LoadUsers();
            Console.WriteLine("[{0}] Starting server...", DateTime.Now);


            string date = DateTime.Now.ToString();
            lock (thisLock)
            {
                fileLogger.WriteLogFile(date + " Starting server..." + "\n");
            }


            if (ConfigurationManager.AppSettings["Protocol"] == "TCP")
                netListener = new NetListener(IPAddress.Any, Convert.ToInt32(ConfigurationManager.AppSettings["Port"]));
            else
                throw new NotImplementedException("Protocols except TCP are have not implemented yet");
            netListener.Start();
            Console.WriteLine("[{0}] Server is running properly!", DateTime.Now);

            lock (thisLock)
            {
                fileLogger.WriteLogFile(date + " Server is running properly! " + "\n");
            }

            Listen();
        }

        public void LoadUsers()
        {
            try
            {
                Console.WriteLine("[{0}] Loading users...", DateTime.Now);

                string date = DateTime.Now.ToString();

                lock (thisLock)
                {
                    fileLogger.WriteLogFile(date + " Loading users..." + "\n");
                }

                BinaryFormatter bf = new BinaryFormatter();
                FileStream file = new FileStream(usersFilename, FileMode.OpenOrCreate, FileAccess.Read);
                if (file.Length > 0)
                {
                    UserInfo[] infos = (UserInfo[])bf.Deserialize(file);
                    file.Close();
                    users = infos.ToDictionary((u) => u.UserName, (u) => u);
                }
                Console.WriteLine("[{0}] Users loaded! ({1})", DateTime.Now, users.Count);

                lock (thisLock)
                {
                    fileLogger.WriteLogFile(date + " Users loaded!" + " " + users.Count.ToString() + "\n");
                }
            }
            catch (Exception e) 
            {
                Console.WriteLine(e.ToString());
            }
        }

        public void SaveUsers()
        {
            try
            {
                Console.WriteLine("[{0}] Saving users...", DateTime.Now);

                string date = DateTime.Now.ToString();

                lock (thisLock)
                {
                    fileLogger.WriteLogFile(date + " Saving users..." + "\n");
                }

                BinaryFormatter bf = new BinaryFormatter();
                FileStream file = new FileStream(usersFilename, FileMode.Create, FileAccess.Write);
                bf.Serialize(file, users.Values.ToArray());
                file.Close();
                Console.WriteLine("[{0}] Users saved!", DateTime.Now);

                lock (thisLock)
                {
                    fileLogger.WriteLogFile(date + " Users saved!" + "\n");
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        void Listen()
        {
            for (;;)
            {
                NetClient netClient = (NetClient) netListener.AcceptClient();
                ClientHandler client = new ClientHandler(netClient);
            }
        }
    }
}
