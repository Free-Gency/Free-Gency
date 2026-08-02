using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Application.Common.Hubs
{
    [Authorize]
    public class ChatHub(ICurrentUserService currentUserService,IUnitOfWork unitOfWork):Hub
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

            await base.OnDisconnectedAsync(exception);
        }
    }
}
