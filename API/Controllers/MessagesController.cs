using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Extentions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    public class MessagesController : BaseApiController
    {
     
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        public MessagesController(IUnitOfWork unitOfWork,IMapper mapper )
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
          
        }

        [HttpPost]
        public async Task<ActionResult<MessageDto>> CreateMessage(CreateMessageDto createMessageDto)
        {
            var username = User.GetUsername();

            if(username == createMessageDto.RecipientUsername.ToLower())
            return BadRequest("cannot send message to yourself");

            var sender = await _unitOfWork.userRepository.GetUserbyUsernameAsync(username);
            var recipient = await _unitOfWork.userRepository.GetUserbyUsernameAsync(createMessageDto.RecipientUsername);

            if(recipient == null) return NotFound();

            var message = new Message
            {
                Sender = sender,
                Recipient = recipient,
                SenderUsername = sender.UserName,
                RecipientUsername = recipient.UserName,
                Content = createMessageDto.Content
            };

            _unitOfWork.messageRepository.AddMessage(message);

            if(await _unitOfWork.Complete()) return Ok(_mapper.Map<MessageDto>(message));
            return BadRequest("FAiled to send message");
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MessageDto>>>GetMessagesForUser([FromQuery]MessageParams messageParams)
        {
            messageParams.UserName = User.GetUsername();

            var messages = await _unitOfWork.messageRepository.GetMessagesForUser(messageParams);

            Response.AddPaginationHeader(messages.CurerntPage,messages.PageSize,messages.TotalCount,messages.TotalPages);

            return messages;
        }

   

        [HttpDelete("{id}")]
        public async Task<ActionResult>DeleteMessage(int id)
        {
            var username = User.GetUsername();
            var message = await _unitOfWork.messageRepository.GetMessage(id);
            
            if(message.Sender.UserName != username && message.Recipient.UserName != username) 
            return  Unauthorized();

            if(message.Sender.UserName == username)  message.SenderDeleted = true; 
            if(message.Recipient.UserName == username)  message.RecipientDeleted = true;

            if(message.RecipientDeleted && message.SenderDeleted) _unitOfWork.messageRepository.DeleteMessage(message);

            if(await _unitOfWork.Complete()) return Ok();

            return BadRequest("message not deleted");

        }
    }
}