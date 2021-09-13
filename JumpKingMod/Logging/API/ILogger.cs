using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logging.API
{
    /// <summary>
    /// An interface representing a Logging object
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// Logs out an information level message
        /// </summary>
        void Information(string message);

        /// <summary>
        /// Logs out an warning level message
        /// </summary>
        void Warning(string message);

        /// <summary>
        /// Logs out an error level message
        /// </summary>
        void Error(string message);
    }
}
