namespace FreeGency.Application.Common.Interfaces
{
    public interface IHirePyEventPublisher
    {
        Task PublishClientEventAsync(
            Guid clientUserId,
            string eventName,
            object payload,
            CancellationToken ct = default);
    }
}
