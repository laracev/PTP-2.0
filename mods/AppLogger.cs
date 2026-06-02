using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Pantry_To_Plate.mods
{
    public static class AppLogger
    {
        private static string LogFilePath = "app_log.txt";


        public static void Log(string message)
        {
            Write($"INFORMATION: {message}");
        }
        public static void LogError(string message)
        {
            Write($"ERROR: {message}");
        }
        public static void LogWarning(string message)
        {
            Write($"WARNING: {message}");
        }

        private static void Write(string message)
        {
            string logEntry = ($"{DateTime.Now}: {message}");
            File.AppendAllText(LogFilePath, logEntry + "\n");
        }

    }
}
