using Logging.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Logging
{
    /// <summary>
    /// An implementation of <see cref="ILogger"/> which initialises a Console and writes the logs to it
    /// </summary>
    public class ConsoleLogger : ILogger
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AllocConsole();

        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        /// <summary>
        /// Default ctor for creating a <see cref="ConsoleLogger"/>
        /// </summary>
        public ConsoleLogger()
        {
            if (GetConsoleWindow() == IntPtr.Zero)
            {
                AllocConsole();
            }
        }

        /// <summary>
        /// Write out an information level message
        /// </summary>
        public void Information(string message)
        {
            string finalMessage = $"INF - {message}";
            Console.WriteLine(finalMessage);
        }

        /// <summary>
        /// Write out an warning level message
        /// </summary>
        public void Warning(string message)
        {
            string finalMessage = $"WAR - {message}";
            Console.WriteLine(finalMessage);
        }

        /// <summary>
        /// Write out an error level message
        /// </summary>
        public void Error(string message)
        {
            string finalMessage = $"ERR - {message}";
            Console.WriteLine(finalMessage);
        }
    }
}
