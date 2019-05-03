using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChiliTester
{
    static class Logger
    {
        private static string logDirectory;

        private static bool headerSet;

        public static void WriteLog(string message, string sender = "Unknown")
        {
            if (logDirectory == null)
            {
                logDirectory = Directory.GetCurrentDirectory() + "\\results";

                if (Directory.Exists(logDirectory) == false)
                {
                    Directory.CreateDirectory(logDirectory);
                }
            }

            FileStream fs = File.OpenWrite(logDirectory + "\\" + sender + ".txt");
            fs.Position = fs.Length;

            message = Environment.NewLine + message;

            if (headerSet == false)
            {
                headerSet = true;

                message = DateTime.Now.Day + "/" + DateTime.Now.Month + "/" + DateTime.Now.Year + " @ " + DateTime.Now.Hour + ":" + DateTime.Now.Minute + ":" + DateTime.Now.Second + Environment.NewLine + message;
                message = "New Log-------------- " + message;

                if (fs.Length > 0)
                {
                    message = Environment.NewLine + Environment.NewLine + "Environment Name: " + sender + ": " + message;
                }
            }

            byte[] messageBytes = Encoding.UTF8.GetBytes(message);

            fs.Write(messageBytes, 0, messageBytes.Length);
            fs.Close();
        }

        public static void WriteLog(Exception exception, string sender = "Unknown")
        {
            string message = exception.Message;

            WriteLog(message, sender);
        }
    }
}
