using System.Collections.Generic;
using System.IO;



namespace FileLoggerLib
{
    public enum LoggerType { SERVER, CLIENT };
    public class FileLogger : IFileLogger
    {
        private string fileName;
        private LoggerType loggerType;
        private List<string> logListClient;

        public FileLogger(string fName, LoggerType lType)
        {
            fileName = fName;
            loggerType = lType;

            if (loggerType == LoggerType.CLIENT)
            {
                logListClient = new List<string>();
            }
        }

        public void WriteLogFile(string info)
        {
            if (loggerType == LoggerType.SERVER)
            {
                FileStream fileStream = new FileStream(fileName, FileMode.Append); //создаем файловый поток для дозаписи в конец файла
                StreamWriter writer = new StreamWriter(fileStream); //создаем «потоковый писатель» и связываем его с файловым потоком 
                writer.Write(info); //записываем в файл
                writer.Close();
            }
            else if (loggerType == LoggerType.CLIENT)
            {
                logListClient.Add(info);
            }
        }

       

        public void CloseLogFile()
        {
            if (loggerType == LoggerType.CLIENT)
            {
                FileStream fileStream = new FileStream(fileName, FileMode.Append);
                StreamWriter writer = new StreamWriter(fileStream);
                foreach (string str in logListClient)
                {
                    writer.Write(str);
                }
                writer.Close();
            }
        }

    }
}
