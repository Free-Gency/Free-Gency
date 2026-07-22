using FreeGency.Application.Common.Errors;
using FreeGency.Application.Common.Exceptions;

namespace FreeGency.Application.Common.Results
{
    public class ApiResponse
    {
        private AppError _error;

        public bool IsSuccess { get; set; }
        public bool IsFailure => !IsSuccess;

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Message { get; } = string.Empty;

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public AppError? Error =>
            _error == AppError.None ? null : _error;

        protected ApiResponse(bool isSuccess, AppError error, string? message = null)
        {
            if (isSuccess && error != AppError.None)
                throw new AppInvalidOperationException("A successful result cannot contain an error.");

            if (!isSuccess && error == AppError.None)
                throw new AppInvalidOperationException("A failed result must contain an error.");

            IsSuccess = isSuccess;
            _error = error;
            Message = message;
        }

        public static ApiResponse Success(string? message = null)
            => new(true, AppError.None, message);

        public static ApiResponse Failure(AppError error, string? message = null)
            => new(false, error, message);

        public static ApiResponse<T> Success<T>(T data, string? message = null)
            => new(data, true, AppError.None, message);

        public static ApiResponse<T> Failure<T>(AppError error, string? message = null)
            => new(default, false, error, message);

    }

    public class ApiResponse<T> : ApiResponse
    {
        private readonly T? _data;

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public T? Data => _data;

        protected internal ApiResponse(T? data, bool isSuccess, AppError error, string? message = null)
            : base(isSuccess, error, message)
        {
            _data = data;
        }

        public static implicit operator ApiResponse<T>(T data) => Success(data);
    }
}
