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

namespace Service
{
    public class Facade
    {
        string usersFilename = Environment.CurrentDirectory + "\\" + ConfigurationManager.AppSettings["UsersFilename"];

        public Dictionary<string, UserInfo> users = new Dictionary<string, UserInfo>();

        INetListener netListener;
        
        public void Init()
        {
            Console.Title = "FlexMessenger Server";
            Console.WriteLine("----- FlexMessenger Server -----");
            LoadUsers();
            Console.WriteLine("[{0}] Starting server...", DateTime.Now);

            if (ConfigurationManager.AppSettings["Protocol"] == "TCP")
                netListener = new NetListener(IPAddress.Any, Convert.ToInt32(ConfigurationManager.AppSettings["Port"]));
            else
                throw new NotImplementedException("Protocols except TCP are have not implemented yet");
            netListener.Start();
            Console.WriteLine("[{0}] Server is running properly!", DateTime.Now);

            Listen();
        }

        public void LoadUsers()
        {
            try
            {
                Console.WriteLine("[{0}] Loading users...", DateTime.Now);
                BinaryFormatter bf = new BinaryFormatter();
                FileStream file = new FileStream(usersFilename, FileMode.Open, FileAccess.Read);
                if (file.Length > 0)
                {
                    UserInfo[] infos = (UserInfo[])bf.Deserialize(file);
                    file.Close();
                    users = infos.ToDictionary((u) => u.UserName, (u) => u);
                }
                Console.WriteLine("[{0}] Users loaded! ({1})", DateTime.Now, users.Count);
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
                BinaryFormatter bf = new BinaryFormatter();
                FileStream file = new FileStream(usersFilename, FileMode.Create, FileAccess.Write);
                bf.Serialize(file, users.Values.ToArray());
                file.Close();
                Console.WriteLine("[{0}] Users saved!", DateTime.Now);
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
