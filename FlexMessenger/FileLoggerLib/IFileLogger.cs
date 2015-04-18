
namespace FileLoggerLib
{
    interface IFileLogger
    {     
        void WriteLogFile(string info);
        void CloseLogFile();
    }
}
