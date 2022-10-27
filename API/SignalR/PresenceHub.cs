using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Extentions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace API.SignalR
{
    public class PresenceHub : Hub
    {
        private readonly PresenceTracker _tracker;
        public PresenceHub(PresenceTracker tracker)
        {
            _tracker = tracker;
        }

        [Authorize]
        public override async Task OnConnectedAsync()       //when connected to hub
        {
            var isOnline = await _tracker.UserConnected(Context.User.GetUsername(),Context.ConnectionId);  // when user connct inject presncetrckr
            if(isOnline)
                await Clients.Others.SendAsync("UserIsOnline",Context.User.GetUsername());  //notifing all 

            var currentUsers = await _tracker.GetOnlineUsers();   // strng onlineusers to currentusers
            await Clients.Caller.SendAsync("GetOnlineUsers",currentUsers);  // sendng list of onlineusers to caller connected
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var isOffline = await _tracker.UserDisconnected(Context.User.GetUsername(),Context.ConnectionId);
            if(isOffline)
                await Clients.Others.SendAsync("UserIsOffline",Context.User.GetUsername());

            // var currentUsers = await _tracker.GetOnlineUsers(); 
            // await Clients.All.SendAsync("GetOnlineUsers",currentUsers);

            await base.OnDisconnectedAsync(exception);  // IF EXCEPTION PASS THAT TO THE BASE OR PARENT CLASS
        }
        
    }
}