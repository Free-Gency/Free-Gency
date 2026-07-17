namespace FoundIt.Application.Common.Models
{
    public record Error(string Code, string Discription, int? StatusCode)
    {
        public static readonly Error None = new Error(string.Empty, string.Empty, null);
    }

}
