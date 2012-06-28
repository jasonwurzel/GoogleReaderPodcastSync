using System;

namespace GoogleReaderAPI2
{
    public class GoogleAuthenticationException : Exception
    {
        public GoogleAuthenticationException() : base(Resource.AuthenticationExceptionMessage) { }
    }
}
