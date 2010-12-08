using System;
using System.Collections.Generic;

namespace OpenFileSystem.IO.FileSystems.Local
{
    public class AggregateException : Exception
    {
        public IEnumerable<Exception> InnerExceptions { get; set; }

        public AggregateException(IEnumerable<Exception> exceptions)
        {
            InnerExceptions = exceptions;
        }
    }
}