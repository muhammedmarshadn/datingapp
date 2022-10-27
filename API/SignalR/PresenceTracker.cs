using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.SignalR
{
    public class PresenceTracker  // want to share among everysingle connectn with server.so adding service as singleton
    {                          //to store key value pairs //list of connection ids -logging from diff devices give diff connection ids
        private static readonly Dictionary<string,List<string>> OnlineUsers =   //DICTNRY IS STORED IN THE MEMRY NOT THE DB
            new Dictionary<string, List<string>>();

        public Task<bool> UserConnected(string username,string connectionId)
        {
            bool isOnline = false;
            lock(OnlineUsers)               //dictnry is shared among everyone connct to server,if users cocurrently try to update we run into prblms
            {                               //so we lock until 
                if(OnlineUsers.ContainsKey(username))   // if we hav a user with 'username'
                {
                    OnlineUsers[username].Add(connectionId);   // adding connctn id to list with key username
                }
                else{                                      // if no user with 'username'
                    OnlineUsers.Add(username,new List<string>{connectionId});  //create a new dicnry with key usernme
                    isOnline = true;
                }
                return Task.FromResult(isOnline);
            }
        }

        public Task<bool> UserDisconnected(string username,string connectionId)
        {
            bool isOffline = false;
            lock(OnlineUsers)
            {
                if(!OnlineUsers.ContainsKey(username)) return Task.FromResult(isOffline);   //no user with key 'username'

                OnlineUsers[username].Remove(connectionId);       //if have that user remove current connection id
                if(OnlineUsers[username].Count == 0)           //if dicnry with key username is empty
                {
                    OnlineUsers.Remove(username);    // remove that element
                    isOffline = true;
                }
            
            }

            return Task.FromResult(isOffline);
        }

        public Task<string[]>GetOnlineUsers()  //retur an array of onlineusers
        {
            string[] onlineUsers;  //VAR TO STORE RESULT 
            lock(OnlineUsers)
            {
                onlineUsers = OnlineUsers.OrderBy(k => k.Key).Select(k=>k.Key).ToArray(); //geting onlineuserS orderby username to the array 'onlineUsers
            }
            return Task.FromResult(onlineUsers);
        }

        public Task<List<string>> GetConnectionsForUser(string username)
        {
            List<string> connectionIds;                                  //list of connection id s 
            lock(OnlineUsers)
            {
                connectionIds = OnlineUsers.GetValueOrDefault(username);   // strng onlineusers connectionids with key username
            }
            return Task.FromResult(connectionIds);                      
        }
    }
}