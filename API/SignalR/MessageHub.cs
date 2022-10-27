using API.DTOs;
using API.Entities;
using API.Extentions;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;

namespace API.SignalR
{
    public class MessageHub : Hub
    {
        private readonly IMapper _mapper;
        private readonly IMessageRepository _messageRepository;
        private readonly IUserRepository _userRepository;
        private readonly IHubContext<PresenceHub> _presenceHub;
        private readonly PresenceTracker _tracker;
        public MessageHub(IMessageRepository messageRepository,IMapper mapper,
                IUserRepository userRepository,IHubContext<PresenceHub> presenceHub,PresenceTracker tracker)
        {
            _tracker = tracker;
            _presenceHub = presenceHub;
            _userRepository = userRepository;
            _messageRepository = messageRepository;
            _mapper = mapper;
        }

        public override async Task OnConnectedAsync(){    // creating a grp for each user (sender(x)- reciver(y) grp)
                                                          //we want to put the user in same grp everytime chating with y person
            var httpContext = Context.GetHttpContext();   // holding httpcontext becoz we need other user's username
            var otherUser = httpContext.Request.Query["user"].ToString(); // passing the username of key(user) to otherUser , to know who the currentuser clicked
            var groupName = GetGroupNmae(Context.User.GetUsername(),otherUser);
            await Groups.AddToGroupAsync(Context.ConnectionId,groupName); // adding user to grp ,if one user or both is connected ,always goes to this grp
            var group = await AddToGroup(groupName);                      //ading conctn to grp
            await Clients.Group(groupName).SendAsync("UpdatedGroup",group); //passing grp back to                        
            var messages = await _messageRepository
                .GetMessageThread(Context.User.GetUsername(),otherUser); //when user joins grp
            await Clients.Caller.SendAsync("RecieveMessageThread",messages);
        }

        public override async Task OnDisconnectedAsync(Exception exception) // when disconnect this method automatically remove them from the grp
        {
            var group = await RemoveFromMessageGroup();
            await Clients.Group(group.Name).SendAsync("UpdatedGroup",group);  //send the updated grp to anyone else still connected in that grp
            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(CreateMessageDto createMessageDto)
        {
             var username =Context.User.GetUsername();

            if(username == createMessageDto.RecipientUsername.ToLower())
            throw new HubException("cannot send message to yourself");

            var sender = await _userRepository.GetUserbyUsernameAsync(username);
            var recipient = await _userRepository.GetUserbyUsernameAsync(createMessageDto.RecipientUsername);

            if(recipient == null) throw new HubException("user not found");

            var message = new Message
            {
                Sender = sender,
                Recipient = recipient,
                SenderUsername = sender.UserName,
                RecipientUsername = recipient.UserName,
                Content = createMessageDto.Content
            };

            var groupName = GetGroupNmae(sender.UserName,recipient.UserName);   // getng the grpname
            var group = await _messageRepository.GetMessageGroup(groupName);    //getng the grp

            if(group.Connections.Any(x=>x.Username == recipient.UserName))     // recient enter grp
            {
                message.DateRead = DateTime.UtcNow;
            }
            else                                              //if user id not in the message tab                
            {
                var connections = await _tracker.GetConnectionsForUser(recipient.UserName); // strng the conctions for users in connections
                if(connections != null)                                                    //checking there is a connection
                {
                    await _presenceHub.Clients.Clients(connections).SendAsync("NewMessageReceived",  // sending msg if user is online but not conncted to grp
                        new{username = sender.UserName, knownAs = sender.KnownAs});
                }
            }
            
            _messageRepository.AddMessage(message);

            if(await _messageRepository.SaveAllAsync()){
                
                await Clients.Group(groupName).SendAsync("NewMessage",_mapper.Map<MessageDto>(message));  //sending msg to grp
                
            }
            
        }

        private async Task<Group> AddToGroup(string groupName)    // hubcallercontext - give access to the current username and connectionid
        {
            var group = await _messageRepository.GetMessageGroup(groupName);                // strng grpnme
            var connection = new Connection(Context.ConnectionId,Context.User.GetUsername()); //strng connctn

            if(group == null)
            {
                group = new Group(groupName);       //crtng new grp adding to db
                _messageRepository.AddGroup(group);
            }
            group.Connections.Add(connection);      // adding conctn

            if(await _messageRepository.SaveAllAsync())  return group;
            throw new HubException("failed to join group");
        }
        

        private async Task<Group> RemoveFromMessageGroup()
        {

            var group =await  _messageRepository.GetGroupForConnection(Context.ConnectionId);  //strng grp for connectn
            var connection = group.Connections.FirstOrDefault(x=>x.ConnectionId == Context.ConnectionId); // getting the connection where user is currently in
            _messageRepository.RemoveConnection(connection);                       //removing cnctn
            if(await _messageRepository.SaveAllAsync()) return group;
            throw new HubException("failed to remove from grp");
        }

        private string GetGroupNmae(string caller, string other){   //this will make grpname in alphebeticorder
            var stringCompare = string.CompareOrdinal(caller,other)<0;
            return stringCompare ? $"{caller}-{other}" : $"{other}-{caller}";
        }
    }
}