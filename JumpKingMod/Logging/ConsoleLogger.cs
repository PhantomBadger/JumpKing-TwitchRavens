using Logging.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Logging
{
    public class ConsoleLogger : ILogger
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AllocConsole();

        public ConsoleLogger()
        {
            AllocConsole();
        }

        public void Information(string message)
        {
            Console.WriteLine($"INF - {message}");
        }

        public void Warning(string message)
        {
            Console.WriteLine($"WAR - {message}");
        }

        public void Error(string message)
        {
            Console.WriteLine($"ERR - {message}");
        }
    }
}
