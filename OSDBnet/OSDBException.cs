using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSDBnet
{
    /// <summary>
    /// OSDB exception
    /// </summary>
    [Serializable]
    public class OsdbException : Exception
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public OsdbException()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message">Message</param>
        public OsdbException(string message) : base(message)
        {
        }
    }
}
