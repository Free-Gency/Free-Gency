using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace FreeGency.Application.Common.Hubs
{
    [Authorize]
    public class ChatHub(
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork,
        OnlineUsersService online) : Hub
    {
        private readonly IUserRepository _userRepository = unitOfWork.Repository<IUserRepository, User>();

        public override async Task OnConnectedAsync()
        {
            var active = await _userRepository.GetActiveProfileAsync(currentUserService.UserId);
            if (active != null)
            {
                var profileId = active.Value.ProfileId;
                var groupName = $"profile-{profileId}";
                Context.Items["ProfileGroup"] = groupName;
                Context.Items["ProfileId"] = profileId;

                await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

                var connections = online.Users.GetOrAdd(profileId, _ => new());
                bool firstConnection;

                lock (connections)
                {
                    connections.Add(Context.ConnectionId);
                    firstConnection = connections.Count == 1;
                }

                if (firstConnection)
                {
                    await Clients.Others.SendAsync("ProfileOnline", profileId.ToString());
                }
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            if (Context.Items.TryGetValue("ProfileGroup", out var groupName))
            {
                await Groups.RemoveFromGroupAsync(
                    Context.ConnectionId,
                    groupName!.ToString()!);
            }

            Guid? profileId = null;
            if (Context.Items.TryGetValue("ProfileId", out var stored)
                && stored is Guid storedId)
            {
                profileId = storedId;
            }
            else
            {
                var active = await _userRepository.GetActiveProfileAsync(currentUserService.UserId);
                profileId = active?.ProfileId;
            }

            if (profileId is Guid pid
                && online.Users.TryGetValue(pid, out var connections))
            {
                bool lastConnection;

                lock (connections)
                {
                    connections.Remove(Context.ConnectionId);
                    if (connections.Count == 0)
                    {
                        online.Users.TryRemove(pid, out _);
                        lastConnection = true;
                    }
                    else
                    {
                        lastConnection = false;
                    }
                }

                if (lastConnection)
                {
                    await Clients.Others.SendAsync("ProfileOffline", pid.ToString());
                }
            }

            await base.OnDisconnectedAsync(exception);
        }

        public async Task IsOnline(Guid profileId)
        {
            await Clients.Caller.SendAsync(
                "OnlineStatus",
                online.Users.ContainsKey(profileId));
        }
    }
}
