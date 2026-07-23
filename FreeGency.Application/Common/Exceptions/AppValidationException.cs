namespace FreeGency.Application.Common.Exceptions
{
    public sealed class AppValidationException : AppException
    {
        public Dictionary<string, string[]> Errors { get; }

        public override int StatusCode => 400;

        public AppValidationException(string field, string message)
            : base("One or more validation errors occurred.")
        {
            Errors = new Dictionary<string, string[]>
            {
                { field, new[] { message } }
            };
        }

        public AppValidationException(IEnumerable<FluentValidation.Results.ValidationFailure> failures)
            : base("One or more validation errors occurred.")
        {
            Errors = failures
                .GroupBy(f => f.PropertyName, f => f.ErrorMessage)
                .ToDictionary(g => g.Key, g => g.ToArray());
        }
    }
}
