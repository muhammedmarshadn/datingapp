using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Extentions;
using API.Helpers;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    public class LikesController : BaseApiController
    {
        private readonly IUnitOfWork _unitOfWork;
       
        public LikesController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
          
        }

        [HttpPost("{username}")]
        public async Task<ActionResult>AddLike(string username)
        {
            var sourceUserId = User.GetUserId();
            var likedUser = await _unitOfWork.userRepository.GetUserbyUsernameAsync(username);
            var sourceUser = await _unitOfWork.likesRepository.GetUserWithLikes(sourceUserId);

            if(likedUser == null)return NotFound();

            if(sourceUser.UserName == username) return BadRequest("you cant like yourself");

            var userLike = await _unitOfWork.likesRepository.GetUserLike(sourceUserId,likedUser.Id);

            if(userLike != null)return BadRequest("You already liked this user");

            userLike = new UserLike
            {
                SourceUserId = sourceUserId,
                LikedUserId = likedUser.Id
            };

            sourceUser.LikedUsers.Add(userLike);

            if(await _unitOfWork.Complete()) return Ok();

            return BadRequest("Failed To LIke");
        }
        public async Task<ActionResult<PagedList<LikeDto>>> GetUserLikes([FromQuery]LikesParams likesParams)
        {
        likesParams.UserId = User.GetUserId();
        var users = await _unitOfWork.likesRepository.GetUserLikes(likesParams);
        Response.AddPaginationHeader(users.CurerntPage,users.PageSize,users.TotalCount,users.TotalPages);
        return Ok(users);
        }
    }
}