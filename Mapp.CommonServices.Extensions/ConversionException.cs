using System;

namespace Shmap.Common
{
    public class ConversionException : Exception
    {
        public ConversionException()
        {
        }

        public ConversionException(string message)
            : base(message)
        {
        }

        public ConversionException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}