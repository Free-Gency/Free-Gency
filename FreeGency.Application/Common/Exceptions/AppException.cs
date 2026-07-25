namespace FreeGency.Application.Common.Exceptions
{
    public abstract class AppException : Exception
    {
        protected AppException(string message) : base(message) { }
        protected AppException(string message, Exception inner) : base(message, inner) { }

        public abstract int StatusCode { get; }
    }
}
