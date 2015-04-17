using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
    public class Facade
    {
        string usersFilename = Environment.CurrentDirectory + "\\" + ConfigurationManager.AppSettings["UsersFilename"];

        public Dictionary<string, UserInfo> users = new Dictionary<string, UserInfo>();

        void Init()
        {
            Console.Title = "FlexMessenger Server";
            Console.WriteLine("----- FlexMessenger Server -----");
            LoadUsers();
            Console.WriteLine("[{0}] Starting server...", DateTime.Now);

            server = new TcpListener(ip, port);
            server.Start();
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
                UserInfo[] infos = (UserInfo[])bf.Deserialize(file);      
                file.Close();
                users = infos.ToDictionary((u) => u.UserName, (u) => u); 
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
    }
}
