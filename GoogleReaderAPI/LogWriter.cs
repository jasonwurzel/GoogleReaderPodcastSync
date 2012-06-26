using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace GoogleReaderAPI
{
    class LogWriter
    {
        static private string LogFilePathAndName = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Desktop Google Reader\\API.txt";
        static private bool isEnabled = false;

        public static void CreateLogfile()
        {
            //if (Properties.Settings.Default.enableLogging)
            if (isEnabled)
            {
                if (File.Exists(LogFilePathAndName))
                {
                    File.Delete(LogFilePathAndName);
                }
                if (!Directory.Exists(Path.GetDirectoryName(LogFilePathAndName)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(LogFilePathAndName));
                }
            }
        }

        public static void WriteTextToLogFile(string text)
        {
            //if (Properties.Settings.Default.enableLogging)
            if(isEnabled)
            {
                try {
                    TextWriter tw = new StreamWriter(LogFilePathAndName, true);
                    tw.WriteLine(DateTime.Now +  ": " + text);
                    tw.Close();

                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
           }
        }

        public static void WriteTextToLogFile(Exception e)
        {
            //if (Properties.Settings.Default.enableLogging)
            if(isEnabled)
            {
                try
                {
                    TextWriter tw = new StreamWriter(LogFilePathAndName, true);
                    tw.WriteLine(DateTime.Now + ": " + e.Message);
                    tw.WriteLine(e.StackTrace);
                    tw.Close();
                    if (e.InnerException != null)
                    {
                        WriteTextToLogFile(e.InnerException);
                    }
                }
                catch (Exception exp)
                {
                    Console.WriteLine(exp.Message);
                }
            }
        }
    }
}
