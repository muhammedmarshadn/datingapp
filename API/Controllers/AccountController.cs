using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    public class AccountController: BaseApiController
    {
        
        private readonly ITokenService _tokenServices;
        private readonly IMapper _mapper;
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        public AccountController(UserManager<AppUser> userManager,SignInManager<AppUser> signInManager , ITokenService tokenServices,IMapper mapper)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _mapper = mapper;
            _tokenServices = tokenServices;
            

        }

        [HttpPost("register")]
        public async Task<ActionResult<UserDto>>Register(RegisterDto registerDto)
        {
            if(await UserExists(registerDto.Username)) return BadRequest("Username is taken");
            
            var user = _mapper.Map<AppUser>(registerDto);
            
            user.UserName = registerDto.Username.ToLower();
           
            var result = await _userManager.CreateAsync(user,registerDto.Password);
            if(!result.Succeeded) return BadRequest(result.Errors);

            var roleResult= await _userManager.AddToRoleAsync(user,"Member");
            if(!roleResult.Succeeded) return BadRequest(result.Errors);
            
            return new UserDto
            {
                Username =user.UserName,
                Token = await _tokenServices.CreateToken(user),
                KnownAs=user.KnownAs,
                Gender= user.Gender
            };
        }
        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
        {
            var user=await _userManager.Users
            .Include(p=> p.Photos)
            .SingleOrDefaultAsync(x => x.UserName == loginDto.Username.ToLower());
            if(user == null) return Unauthorized("invalid username");

            var result = await _signInManager
                    .CheckPasswordSignInAsync(user,loginDto.Password,false);

            if(!result.Succeeded) return Unauthorized();

           
            return new UserDto
            {
                Username =user.UserName,
                Token = await _tokenServices.CreateToken(user),
                PhotoUrl= user.Photos.FirstOrDefault(x=> x.IsMain)?.Url,
                KnownAs= user.KnownAs,
                Gender= user.Gender
            };
        }


        private async Task<bool>UserExists(string Username)
        {
            return await _userManager.Users.AnyAsync(x =>x.UserName == Username.ToLower());
        }
    }
}