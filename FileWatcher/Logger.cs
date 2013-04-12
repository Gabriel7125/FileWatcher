using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace FileWatcher
{
    public static class Logger
    {
        public static void Error(string message, string module)
        {
            WriteEntry(message, "error", module);
        }

        public static void Error(Exception ex, string module)
        {
            WriteEntry(ex.Message, "error", module);
        }

        public static void Warning(string message, string module)
        {
            WriteEntry(message, "warning", module);
        }

        public static void Info(string message, string module)
        {
            WriteEntry(message, "info", module);
        }

        public static void ApplicationExiting()
        {
            Logger.Info("Bye!", "OnExit");
            Trace.WriteLine("--------------------------------------------------");
        }

        private static void WriteEntry(string message, string type, string module)
        {
            Trace.WriteLine(string.Format("{0},{1},{2},{3}",
                              DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                              type,
                              module,
                              message));
        }
    }
}
