using System;

namespace KinopoiskUWP.Services
{
    public class KinopoiskApiException : Exception
    {
        public KinopoiskApiException() { }

        public KinopoiskApiException(string message)
            : base(message) { }

        public KinopoiskApiException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}