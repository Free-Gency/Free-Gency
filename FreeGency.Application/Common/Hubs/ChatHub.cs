using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
namespace FreeGency.Application.Common.Hubs
{
    [Authorize]
    public class ChatHub(ICurrentUserService currentUserService,IUnitOfWork unitOfWork,OnlineUsersService online):Hub
    {
        private readonly IUserRepository _userRepository = unitOfWork.Repository<IUserRepository, User>();
        public static readonly ConcurrentDictionary<Guid, ConcurrentDictionary<Guid, byte>> ActiveRoomUsers = new();
        public override async Task OnConnectedAsync()
        {
            var active = await _userRepository.GetActiveProfileAsync(currentUserService.UserId);
            if (active != null)
            {
                var groupName = $"profile-{active.Value.ProfileId}";
                Context.Items["ProfileGroup"] = groupName;

                await Groups.AddToGroupAsync(
                    Context.ConnectionId,
                    groupName
                );
                var profileId = active.Value.ProfileId;

                var connections = online.Users.GetOrAdd(profileId, _ => new());

                bool firstConnection = false;

                lock (connections)
                {
                    connections.Add(Context.ConnectionId);

                    firstConnection = connections.Count == 1;
                }

                if (firstConnection)
                {
                    await Clients.All.SendAsync(
                        "ProfileOnline",
                        profileId
                    );
                }
            } 
            await base.OnConnectedAsync();
        }
        public static bool IsUserInRoom(Guid roomId, Guid profileId)
        {
            return ActiveRoomUsers.TryGetValue(roomId, out var users)
                   && users.ContainsKey(profileId);
        }
        public async Task JoinRoom(Guid roomId)
        {
            var active = await _userRepository.GetActiveProfileAsync(currentUserService.UserId);

            await Groups.AddToGroupAsync(Context.ConnectionId, $"room-{roomId}");

            var users = ActiveRoomUsers.GetOrAdd(roomId, _ => new());

            users[active!.Value.ProfileId] = 0;
            Console.WriteLine($"Join {Context.ConnectionId} => room-{roomId}");
        }
        public async Task LeaveRoom(Guid roomId)
        {
            var active = await _userRepository.GetActiveProfileAsync(currentUserService.UserId);

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"room-{roomId}");

            if (ActiveRoomUsers.TryGetValue(roomId, out var users))
            {
                users.TryRemove(active!.Value.ProfileId, out _);

                if (users.IsEmpty)
                    ActiveRoomUsers.TryRemove(roomId, out _);
            }
        }
        public async override Task OnDisconnectedAsync(Exception? exception)
        {
            if (Context.Items.TryGetValue(
            "ProfileGroup",
            out var groupName))
            {

                await Groups.RemoveFromGroupAsync(
                    Context.ConnectionId,
                    groupName!.ToString()!
                );
            }
            var active = await _userRepository.GetActiveProfileAsync(currentUserService.UserId);
            if (active != null)
            {
                var profileId = active.Value.ProfileId;
                if (online.Users.TryGetValue(profileId, out var connections))
                {
                    bool lastConnection = false;

                    lock (connections)
                    {
                        connections.Remove(Context.ConnectionId);

                        if (connections.Count == 0)
                        {
                            online.Users.TryRemove(profileId, out _);
                            lastConnection = true;
                        }
                    }

                    if (lastConnection)
                    {
                        await Clients.All.SendAsync(
                            "ProfileOffline",
                            profileId
                        );
                    }
                }
            }
                await base.OnDisconnectedAsync(exception);
        }
        public async Task IsOnline(Guid profileId)
        {
            await Clients.Caller.SendAsync(
                "OnlineStatus",
                online.Users.ContainsKey(profileId)
            );
        }
    }
}
