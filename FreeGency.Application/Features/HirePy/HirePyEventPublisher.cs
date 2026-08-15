using Microsoft.AspNetCore.SignalR;
using FreeGency.Application.Common.Hubs;
using FreeGency.Application.Common.Interfaces;

namespace FreeGency.Application.Features.HirePy;

public class HirePyEventPublisher : IHirePyEventPublisher
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly IUserRepository _userRepository;

    public HirePyEventPublisher(
        IHubContext<NotificationHub> hubContext,
        IUserRepository userRepository)
    {
        _hubContext = hubContext;
        _userRepository = userRepository;
    }

    public async Task PublishClientEventAsync(
        Guid clientUserId,
        string eventName,
        object payload,
        CancellationToken ct = default)
    {
        var clientProfileId = await _userRepository.GetClientProfileIdByUserIdAsync(clientUserId, ct);
        if (clientProfileId is null)
            return;

        var connections = NotificationHub.GetConnections(clientProfileId.Value);
        if (connections.Count == 0)
            return;

        await _hubContext.Clients.Clients(connections).SendAsync(eventName, payload, ct);
    }
}
