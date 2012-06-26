using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace GoogleReaderAPI
{
    public class GoogleAuthenticationException : Exception
    {
        public GoogleAuthenticationException() : base(Resources.AuthenticationExceptionMessage) { }
    }
}
