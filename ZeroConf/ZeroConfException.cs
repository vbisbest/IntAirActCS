using System;
using System.Collections.Generic;
using System.Text;

namespace ZeroConf
{
    public class ZeroConfException : Exception
    {
        public ZeroConfException() { }
        public ZeroConfException(string message) : base(message) { }
        public ZeroConfException(string message, Exception inner) : base(message, inner) { }
    }
}