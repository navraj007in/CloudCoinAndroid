using System;
using System.IO;

namespace CloudCoinCore
{
    [Flags]
    public enum LogLevel
    {
        TRACE,
        INFO,
        DEBUG,
        WARNING,
        ERROR,
        FATAL
    }

    public abstract class Logger
    {
        public abstract void DisplayMessage(String message,LogLevel logLevel= LogLevel.INFO,bool writeToLog = false);
    }
    public class CoreLogger
    {
        //static string assemblyFile = (new System.Uri(Directory.GetCurrentDirectory())).AbsolutePath;

      
        //Fields 
        //static string basedir = assemblyFile + Path.DirectorySeparatorChar;
        static string logFolder; // = basedir + "Logs" + Path.DirectorySeparatorChar;
        static string logFile;
        
        
        //static string coinutilsLogFile = logFolder + "coinutils.log";
        //static string detectionagentLogFile = logFolder + "detectionagent.log";
        //static string detectorLogFile = logFolder + "detector.log";
        //static string dumperLogFile = logFolder + "dumper.log";
        //static string WithdrawerLogFile = logFolder + "Withdrawer.log";
        //static string fileutilsLogFile = logFolder + "fileutils.log";
        //static string frack_fixerLogFile = logFolder + "frack_fixer.log";
        //static string DepositerLogFile = logFolder + "Depositer.log";
        //static string raidaLogFile = logFolder + "raida.log";





        //Constructors
        
        public static void initCoreLogger(string logfolder)
        {
            logFolder = logfolder;
            logFile = logfolder + "logs" + DateTime.Now.ToString("yyyyMMdd").ToLower() + ".log";
        }


        static void createDir()
        {
            try
            {
                if (Directory.Exists(logFolder) == false)
                {
                    Directory.CreateDirectory(logFolder);
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }


        }


        public static async void Log(string message, [System.Runtime.CompilerServices.CallerFilePath] string classpath = "")
        {
            try
            {
                createDir();

                TextWriter tw = File.AppendText(logFile);
                using (tw)
                {
                    await tw.WriteLineAsync(DateTime.Now.ToString());
                    await tw.WriteLineAsync(message);

                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
            
            
        }

        
    }
}
