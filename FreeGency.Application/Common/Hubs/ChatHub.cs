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
                var GroupName = $"profile-{active.Value.ProfileId}";
                await Groups.AddToGroupAsync(Context.ConnectionId, GroupName);
            } 
            await base.OnConnectedAsync();
        }
        public async override Task OnDisconnectedAsync(Exception? exception)
        {
            var active = await _userRepository.GetActiveProfileAsync(currentUserService.UserId);
            if (active != null)
            {
                var GroupName = $"profile-{active.Value.ProfileId}";
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupName);
            }
            await base.OnDisconnectedAsync(exception);
        }
    }
}
