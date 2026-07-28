using FreeGency.Domain.Specifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Application.Common.Hubs
{
    [Authorize]
    public class NotificationHub(IUnitOfWork unitOfWork, ICurrentUserService currentUserService) : Hub
    {
        public static readonly ConcurrentDictionary<Guid, HashSet<string>> UserConnections = new();
        private readonly IClientProfileRepository clientProfileRepository = unitOfWork.Repository<IClientProfileRepository, ClientProfile>();
        private readonly IDeveloperProfileRepository developerProfileRepository = unitOfWork.Repository<IDeveloperProfileRepository, DeveloperProfile>();
        private readonly IUserRepository userRepository = unitOfWork.Repository<IUserRepository, User>();
        public override async Task OnConnectedAsync()
        {
            var userId = currentUserService.UserId;
            if (userId != Guid.Empty)
            {
                var user = await userRepository.GetByIdAsync(userId);
                if (user != null)
                {
                    if (user.ActiveProfileMode == profileMode.Client)
                    {
                        var clientprofile = await clientProfileRepository.GetEntityWithSpec(new ClientAccountSpecifiaction(userId));
                        AddConnection(clientprofile!.Id, Context.ConnectionId);
                    }
                    else
                    {
                        var devloperprofile = await developerProfileRepository.GetEntityWithSpec(new DeveloperAccountSpecification(userId));
                        AddConnection(devloperprofile!.Id, Context.ConnectionId);
                    }
                }
            }
            await base.OnConnectedAsync();
            return;

        }
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = currentUserService.UserId;

            if (userId != Guid.Empty)
            {
                var user = await userRepository.GetByIdAsync(userId);

                if (user != null)
                {
                    Guid profileId;

                    if (user.ActiveProfileMode == profileMode.Client)
                    {
                        var profile = await clientProfileRepository
                            .GetEntityWithSpec(new ClientAccountSpecifiaction(userId));

                        profileId = profile.Id;
                    }
                    else
                    {
                        var profile = await developerProfileRepository
                            .GetEntityWithSpec(new DeveloperAccountSpecification(userId));

                        profileId = profile.Id;
                    }

                    if (UserConnections.TryGetValue(profileId, out var connections))
                    {
                        lock (connections)
                        {
                            connections.Remove(Context.ConnectionId);

                            if (connections.Count == 0)
                            {
                                UserConnections.TryRemove(profileId, out _);
                            }
                        }
                    }
                }
            }

            await base.OnDisconnectedAsync(exception);
        }
        public static List<string> GetConnections(Guid profileId)
        {
            if (UserConnections.TryGetValue(profileId, out var connections))
            {
                lock (connections)
                {
                    return connections.ToList();
                }
            }

            return [];
        }
        private static void AddConnection(Guid profileId, string connectionId)
        {
            if (!UserConnections.TryGetValue(profileId, out var connections))
            {
                connections = new HashSet<string>();
                UserConnections[profileId] = connections;
            }

            lock (connections)
            {
                connections.Add(connectionId);
            }
        }
    }
}

