using System;
using Microsoft.AspNetCore.Mvc;
using Backend.Entities.Models;
using Backend.Payloads;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;
using Backend.Services;
using Microsoft.AspNetCore.Http;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {

            private IConfiguration _config { get; }
            private readonly IAccountService _accountService;
            private readonly IRepositoryManager _repositoryManager;

            public AccountController(IConfiguration configuration, IAccountService accountService, IRepositoryManager repositoryManager)
                {
                    _config = configuration;
                    _accountService = accountService;
                    _repositoryManager = repositoryManager;
                }

            protected int getUserId()
            {
                return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            }

        [AllowAnonymous]
        [HttpPost("register")]
        public IActionResult register([FromBody] RegisterPayload registerPayload)
        {
            try
            {
                _accountService.Register(registerPayload);
                return new JsonResult(new { status = true, message = "Account created succesfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }          

        }

        [AllowAnonymous]
        [HttpGet("confirmEmail")]
        public IActionResult confirmEmail(string email, string token)
        {
            try
            {
                _accountService.confirmEmail(email, token);
                return new JsonResult(new { message = "Email confirmed" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            } 
        }

        [AllowAnonymous]
        [Route("login")]
        [HttpPost]
        public IActionResult login([FromBody] LoginPayload loginPayload)
        {
            try
            {
                var token = _accountService.login(loginPayload);
                return new JsonResult(new { token = token });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            } 
        }

        [Authorize(Roles = Role.Admin)]
        [HttpPatch("changePassword")]
        public IActionResult changePassword(string email, string newPassword)
        {
            try
            {
                _accountService.changePassword(email, newPassword);
                return new JsonResult(new { status = "Password change successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            } 
            
        }

        [Authorize]
        [HttpPatch("changeUserPassword")]
        public IActionResult changeUserPassword(string newPassword, string oldPassword)
        {
            try
            {
                _accountService.changeUserPassword(newPassword, oldPassword, getUserId());
                return new JsonResult(new { status = true });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            } 
        }

        [Authorize]
        [HttpPatch("changeUserEmail")]
        public IActionResult changeUserEmail(string newEmail, string password)
        {
            try
            {
                _accountService.changeUserEmail(newEmail, password, getUserId());
                return new JsonResult(new { status = "Success" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            } 
            
        }
        [Authorize]
        [HttpPost("addPhoto")]
        public IActionResult addPhoto()
        {
            IFormFile file = Request.Form.Files[0];
            var url = _accountService.addProfilePhoto(getUserId(), file);
            return new JsonResult(new { status = "ok", uri = url });
        }

        [HttpGet("getUser")]
        public IActionResult getUser(long userID)
        {
            var foundUser = _accountService.getUser(userID);
            return new JsonResult(new { user = foundUser });
        }

        [HttpGet("getProfilePicture")]
        public IActionResult getProfilePicture(long userID)
        {
            var url = _repositoryManager.getProfilePicture(userID);
            return new JsonResult(new { imgLink = url });
        }

        [HttpGet("forgotPassword")]
        public IActionResult forgotPassword(string email)
        {
            try
            {
                _accountService.forgotPassword(email);
                return new JsonResult(new { message = "An email to reset the password was sent"});
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            } 
            
        }

        [HttpPatch("resetPassword")]
        public IActionResult resetPassword(string email, string token, string newPassword)
        {
            try
            {
                _accountService.resetPassword(email, token, newPassword);
                return new JsonResult(new { message = "Password Sucessfully Changed!", status = true });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            } 
            
        }

        [HttpGet("getMyProfilePicture")]
        public IActionResult getMyProfilePicture()
        {
            var url = _repositoryManager.getProfilePicture(getUserId());
            return new JsonResult(new { imgLink = url });
        }
        [Authorize]
        [HttpDelete("deleteProfilePicture")]
        public IActionResult deleteProfilePicture()
        {
            _accountService.deleteProfilePicture(getUserId());
            return new JsonResult(new { status = "Success", ok = "true" });

        }

        [Authorize]
        [HttpGet("getName")]
        public IActionResult getName()
        {
            var name = _accountService.getName(getUserId());
            return new JsonResult(new { name = name });
        }

    }

}
