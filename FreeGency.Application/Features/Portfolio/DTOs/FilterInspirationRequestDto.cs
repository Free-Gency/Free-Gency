namespace FreeGency.Application.Features.Portfolio.DTOs
{
    public sealed class FilterInspirationRequestDto : PagedQuery
    {
        public Guid? CategoryId { get; init; }
        public string? Search { get; init; }
    }
}
