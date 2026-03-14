using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Security.Claims;

namespace RecruitmentInterviewManagementSystem.Applications.TaiOrXiuFeature.HubResult
{
    [Authorize]
    public class TaiXiuHub : Hub
    {
        private const string ROOM = "taixiu-room";

        // userId -> list connection
        private static ConcurrentDictionary<string, HashSet<string>> OnlineUsers
            = new();

        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                         ?? Context.ConnectionId;

            var connectionId = Context.ConnectionId;

            await Groups.AddToGroupAsync(connectionId, ROOM);

            OnlineUsers.AddOrUpdate(
                userId,
                new HashSet<string> { connectionId },
                (key, oldSet) =>
                {
                    lock (oldSet)
                    {
                        oldSet.Add(connectionId);
                    }
                    return oldSet;
                });

            await Clients.Group(ROOM).SendAsync("OnlineCount", OnlineUsers.Count);

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                         ?? Context.ConnectionId;

            var connectionId = Context.ConnectionId;

            if (OnlineUsers.TryGetValue(userId, out var connections))
            {
                lock (connections)
                {
                    connections.Remove(connectionId);

                    if (connections.Count == 0)
                    {
                        OnlineUsers.TryRemove(userId, out _);
                    }
                }
            }

            await Groups.RemoveFromGroupAsync(connectionId, ROOM);

            await Clients.Group(ROOM).SendAsync("OnlineCount", OnlineUsers.Count);

            await base.OnDisconnectedAsync(exception);
        }
    }
}