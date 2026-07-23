namespace FreeGency.Application.Common.Exceptions
{
    public class AppInvalidOperationException : AppException
    {
        public override int StatusCode => 500;

        public AppInvalidOperationException(string message)
            : base(message) { }
    }
}