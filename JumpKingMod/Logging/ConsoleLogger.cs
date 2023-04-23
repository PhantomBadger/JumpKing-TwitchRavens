using Logging.API;
using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.IO;
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
        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("kernel32.dll",
            EntryPoint = "AllocConsole",
            SetLastError = true,
            CharSet = CharSet.Auto,
            CallingConvention = CallingConvention.StdCall)]
        private static extern int AllocConsole();

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr CreateFile(
            string lpFileName,
            uint dwDesiredAccess,
            uint dwShareMode,
            uint lpSecurityAttributes,
            uint dwCreationDisposition,
            uint dwFlagsAndAttributes,
            uint hTemplateFile);

        private const int MY_CODE_PAGE = 437;
        private const uint GENERIC_WRITE = 0x40000000;
        private const uint FILE_SHARE_WRITE = 0x2;
        private const uint OPEN_EXISTING = 0x3;

        /// <summary>
        /// Singleton of the Console Logger
        /// </summary>
        public static ConsoleLogger Instance 
        { 
            get 
            { 
                if (instance == null)
                {
                    instance = new ConsoleLogger();
                }
                return instance;
            } 
        }
        private static ConsoleLogger instance;

        /// <summary>
        /// Default ctor for creating a <see cref="ConsoleLogger"/>
        /// </summary>
        public ConsoleLogger()
        {
            if (GetConsoleWindow() == IntPtr.Zero)
            {
                // JK+ Redirects stdout to something, so we'll forcibly steal it back
                // we should confirm with them what they're using it for, it may be
                // that we should defer to using a log file instead
                AllocConsole();

                IntPtr stdHandle = CreateFile(
                    "CONOUT$",
                    GENERIC_WRITE,
                    FILE_SHARE_WRITE,
                    0, OPEN_EXISTING, 0, 0
                );

                SafeFileHandle safeFileHandle = new SafeFileHandle(stdHandle, true);
                FileStream fileStream = new FileStream(safeFileHandle, FileAccess.Write);
                Encoding encoding = System.Text.Encoding.GetEncoding(MY_CODE_PAGE);
                StreamWriter standardOutput = new StreamWriter(fileStream, encoding);
                standardOutput.AutoFlush = true;
                Console.SetOut(standardOutput);
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
