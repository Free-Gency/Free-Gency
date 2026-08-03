using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Application.Common.Hubs
{
    [Authorize]
    public class ChatHub(ICurrentUserService currentUserService,IUnitOfWork unitOfWork,OnlineUsersService online):Hub
    {
        private readonly IUserRepository _userRepository = unitOfWork.Repository<IUserRepository, User>();
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
