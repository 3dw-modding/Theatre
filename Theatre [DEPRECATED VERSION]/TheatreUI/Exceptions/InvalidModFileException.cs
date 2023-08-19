using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Theatre.Exceptions
{
    internal class InvalidModFileException : Exception
    {
        public InvalidModFileException(string message) : base(message){ }
        public InvalidModFileException(string message, Exception innerException) : base(message, innerException) { }
    }
}
