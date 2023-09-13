using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TopNewsApi.Core.DTOs.User;
using TopNewsApi.Core.Services;
using TopNewsApi.Core.Validation.User;

namespace TopNewsApi.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;
        public UserController(UserService userService)
        {
            _userService = userService;
        }
        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            return Ok((await _userService.GetAllAsync()).Payload);
        }

        [HttpPost("AddUser")]
        public async Task<IActionResult> Create(CreateUserDto model)
        {
            var validator = new CreateUserValidation();
            var validationResult = await validator.ValidateAsync(model);
            if (validationResult.IsValid)
            {
                var result = await _userService.CreateNewUserAsync(model);
                if (result.Success)
                {
                    return Ok("User created!");
                }
                else 
                {
                    return BadRequest("Something gone wrong!");
                }
            }
            return BadRequest(validationResult.Errors);
        }
        [HttpPost("DeleteUser")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            ServiceResponse result = await _userService.DeleteUserAsync(id);
            if (result.Success)
            {
                return Ok(result.Message);
            }
            return Ok(result.Errors.FirstOrDefault());
        }
    }
}
