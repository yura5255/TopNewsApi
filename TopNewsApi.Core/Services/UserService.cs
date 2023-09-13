using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TopNewsApi.Core.DTOs.User;
using TopNewsApi.Core.Entities.User;

namespace TopNewsApi.Core.Services
{
    public class UserService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly EmailService _emailService;
        public UserService(UserManager<AppUser> userManager, IMapper mapper, EmailService emailService, IConfiguration configuration)
        {
            _userManager = userManager;
            _mapper = mapper;
            _emailService = emailService;
            _configuration= configuration;
        }
        public async Task<ServiceResponse> GetAllAsync()
        {
            List<AppUser> users = await _userManager.Users.ToListAsync();
            List<UserDTO> mappedUsers = users.Select(u => _mapper.Map<AppUser, UserDTO>(u)).ToList();
            return new ServiceResponse
            {
                Success = true,
                Message = "All users loaded!",
                Payload = mappedUsers,
            };
        }

        public async Task<ServiceResponse> CreateNewUserAsync(CreateUserDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user != null)
            {
                return new ServiceResponse
                {
                    Success = false,
                    Message = "User already exists!"
                };
            }
            AppUser mappedUser = _mapper.Map<CreateUserDto, AppUser>(model);
            IdentityResult result = await _userManager.CreateAsync(mappedUser, model.Password);
            if (result.Succeeded)
            {
                _userManager.AddToRoleAsync(mappedUser, model.Role).Wait();
                //await _emailService.SendEmail(model.Email, "Hello!", "Hello World!");
                await SendConfirmationEmailAsync(mappedUser);
                return new ServiceResponse
                {
                    Success = true,
                    Message = "User succesfully created!"
                };
            }
            List<IdentityError> errorList = result.Errors.ToList();
            string errors = "";
            foreach (var error in errorList)
            {
                errors = error + error.Description.ToString();
            }
            return new ServiceResponse
            {
                Success = false,
                Message = "Error",
                Payload = errors
            };
        }
        public async Task SendConfirmationEmailAsync(AppUser user)
        {
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var encodedToken = Encoding.UTF8.GetBytes(token);
            var validEmailToken = WebEncoders.Base64UrlEncode(encodedToken);
            string url = $"{_configuration["HostSettings:URL"]}/Dashboard/ConfirmEmail?userId={user.Id}&token={validEmailToken}";
            string emailBody = $"<h1>Confirm your email please!</h1><a href='{url}'>Confirm now</a>";
            await _emailService.SendEmail(user.Email, "Email confirmation!", emailBody);
        }

        public async Task<ServiceResponse> DeleteUserAsync(string Id)
        {
            AppUser userdelete = await _userManager.FindByIdAsync(Id);
            if (userdelete == null)
            {
                return new ServiceResponse
                {
                    Success = false,
                    Message = "Error",
                };
            }
            IdentityResult result = await _userManager.DeleteAsync(userdelete);
            if (result.Succeeded)
            {
                return new ServiceResponse
                {
                    Success = true,
                };
            }
            return new ServiceResponse
            {
                Success = false, 
                Message = "something went wrong", 
            };
        }

    }
}
