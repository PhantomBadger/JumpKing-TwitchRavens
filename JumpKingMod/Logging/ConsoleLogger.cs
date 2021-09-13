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

        /// <summary>
        /// Default ctor for creating a <see cref="ConsoleLogger"/>
        /// </summary>
        public ConsoleLogger()
        {
            AllocConsole();
        }

        /// <summary>
        /// Write out an information level message
        /// </summary>
        public void Information(string message)
        {
            Console.WriteLine($"INF - {message}");
        }

        /// <summary>
        /// Write out an warning level message
        /// </summary>
        public void Warning(string message)
        {
            Console.WriteLine($"WAR - {message}");
        }

        /// <summary>
        /// Write out an error level message
        /// </summary>
        public void Error(string message)
        {
            Console.WriteLine($"ERR - {message}");
        }
    }
}
