using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace JumpKingMod.Install.UI
{
    public class FileUnblocker
    {
        [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteFile(string name);

        public const int ERROR_SUCCESS = 0;
        public const int ERROR_FILE_NOT_FOUND = 2;
        public const int ERROR_ACCESS_DENIED = 5;

        /// <summary>
        /// Attempt to unblock the file,
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        public bool TryUnblockFile(string fileName, out string error, out int errorCode)
        {
            bool result = DeleteFile(fileName + ":Zone.Identifier");
            error = "";
            errorCode = 0;

            // Handle any errors
            if (!result)
            {
                errorCode = Marshal.GetLastWin32Error();

                switch (errorCode)
                {
                    case ERROR_SUCCESS:
                        error = "";
                        break;
                    case ERROR_FILE_NOT_FOUND:
                        error = $"Unable to unblock '{fileName}' as the file was not found!";
                        break;
                    case ERROR_ACCESS_DENIED:
                        error = $"Unable to unblock '{fileName}' as access was denied!";
                        break;
                    default:
                        error = $"Unable to unblock '{fileName}' due to unknown error '{errorCode}'";
                        break;
                }
            }

            return result;
        }
    }
}
