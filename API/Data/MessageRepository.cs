using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using API.Extentions;

namespace API.Data
{
    public class MessageRepository : IMessageRepository
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        public MessageRepository(DataContext context , IMapper mapper)
        {
            _mapper = mapper;
            _context = context;
        }

        public void AddGroup(Group group)
        {
            _context.Groups.Add(group);       //adding the group to db
        }

        public void AddMessage(Message message)
        {
            _context.Messages.Add(message);
        }

        public void DeleteMessage(Message message)
        {
            _context.Messages.Remove(message);
        }

        public async Task<Connection> GetConnection(string connectionId)
        {
            return await _context.Connections.FindAsync(connectionId);
        }

        public async Task<Group> GetGroupForConnection(string connectionId)   // return a grp for the connectionId
        {
            return await _context.Groups
                .Include(c=> c.Connections)
                .Where(c=>c.Connections.Any(x=>x.ConnectionId == connectionId))  //checking connectionids are = or !
                .FirstOrDefaultAsync();
        }

        public async Task<Message> GetMessage(int id)
        {
           return await _context.Messages
           .Include(u=>u.Recipient)
           .Include(u=>u.Sender)
           .SingleOrDefaultAsync(x=>x.Id == id);
        }

        public async Task<Group> GetMessageGroup(string groupName)
        {
            return await _context.Groups
                    .Include(x=>x.Connections)                        //to get connections
                    .FirstOrDefaultAsync(x=>x.Name == groupName);     // "" grpname
        }

        public async Task<PagedList<MessageDto>>GetMessagesForUser(MessageParams messageParams)
        {
            var query = _context.Messages
                .OrderByDescending(m=> m.MessageSent)
                .ProjectTo<MessageDto>(_mapper.ConfigurationProvider)
                .AsQueryable();

            query = messageParams.Container switch
            {
                "Inbox" => query.Where(u=>u.RecipientUsername == messageParams.UserName 
                            && u.RecipientDeleted == false),
                "Outbox" => query.Where(u=>u.SenderUsername == messageParams.UserName 
                            && u.SenderDeleted == false),
                _=>query.Where(u=>u.RecipientUsername == messageParams.UserName
                            && u.RecipientDeleted == false && u.DateRead== null)
            };

            return await PagedList<MessageDto>.CreateAsync(query,messageParams.PageNumber,messageParams.PageSize);
        }

        public async Task<IEnumerable<MessageDto>> GetMessageThread(string currentUsername,string recipientUsername)
         {

        //     var unreadMessages = await _context.Messages.Where(m=>m.DateRead == null
        //     && m.RecipientUsername == currentUsername).ToListAsync();
 
        //     if(unreadMessages.Any())
        //     {
        //         foreach(var message in unreadMessages)    // MarkUreadAsRead TAKE CARE OF THIS 
        //         {
        //             message.DateRead = DateTime.UtcNow;
        //         }
                
        //     }
            
            var messages = await _context.Messages                    // gtng messges
                .Where(m=>m.Recipient.UserName == currentUsername && m.RecipientDeleted == false  // checking names and msg is not deleted
                        && m.Sender.UserName == recipientUsername
                        || m.Recipient.UserName == recipientUsername
                        && m.Sender.UserName == currentUsername && m.SenderDeleted == false
                ).MarkUreadAsRead(currentUsername)
                .OrderBy(m=>m.MessageSent)
                .ProjectTo<MessageDto>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return messages;
        }

        public void RemoveConnection(Connection connection)
        {
            _context.Connections.Remove(connection);
        }

       
    }
}