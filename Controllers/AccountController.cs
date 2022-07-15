using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Backend.Entities;
using Backend.Entities.Models;
using Backend.Payloads;
using Microsoft.AspNetCore.Authorization;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using MimeKit;
using MailKit.Security;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {

            private IConfiguration _config { get; }
            private readonly BackendContext _db;

            public AccountController(BackendContext db, IConfiguration configuration)
                {
                    _config = configuration;
                    _db = db;
                }

            protected int getUserId()
            {
                return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<ActionResult<User>> Register([FromBody] RegisterPayload registerPayload)
        {

            try
            {
                MailAddress m = new MailAddress(registerPayload.Email);
            }
            catch (Exception ex)
            {

                return new JsonResult(new { status = "false", message = "email format " + ex.Message });
            }


            try
            {
                var existingUserWithMail = _db.Users
            .Any(u => u.Email == registerPayload.Email);


                if (existingUserWithMail)
                {
                    return Conflict(new { status = false, message = "Email Exists" });
                }

                var userToCreate = new User
                {
                    Email = registerPayload.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerPayload.Password),
                    Firstname = registerPayload.Firstname,
                    Lastname = registerPayload.Lastname,
                    Role = "User",
                    Phonenumber = registerPayload.Phonenumber,
                    availableListings = 5,
                    isConfirmed = false,
                };

                _db.Users.Add(userToCreate);

                userToCreate.clientURI = GenerateJSONWebToken(userToCreate);

                _db.SaveChanges();

                using var message = new MimeMessage();
                message.From.Add(new MailboxAddress(
                    "WorldWide Cars Trade",
                    "ww.cars.trade@gmail.com"
                ));
                message.To.Add(new MailboxAddress(
                    userToCreate.Firstname + " " + userToCreate.Lastname,
                    userToCreate.Email
                ));
                message.Subject = "Email Address Activation";
                var bodyBuilder = new BodyBuilder
                {
                    TextBody = "Hello, welcome to WorldWide Cars Trade. Please use the link below to activate your account. \n" +
                    "http://localhost:4200/confirm?token=" + userToCreate.clientURI + "&email=" + userToCreate.Email,
                };
                message.Body = bodyBuilder.ToMessageBody();

                using var client = new MailKit.Net.Smtp.SmtpClient();
                // SecureSocketOptions.StartTls force a secure connection over TLS
                await client.ConnectAsync("smtp.sendgrid.net", 587, SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(
                    userName: "apikey", // the userName is the exact string "apikey" and not the API key itself.
                    password: "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" // password is the API key
                );
                await client.SendAsync(message);

                await client.DisconnectAsync(true);

                return Ok(new { status = true, message = "Account created succesfully", user = userToCreate });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { status = "false", message = "" + ex.Message });
            }

        }

        [AllowAnonymous]
        [HttpGet("confirmEmail")]
        public IActionResult confirmEmail(string email, string token)
        {
            var foundUser = _db.Users.Where(u => u.Email == email).SingleOrDefault();
            if (foundUser == null) return BadRequest(new { status = false, message = "User not found" });
            else
            {
                if(foundUser.isConfirmed == true) return BadRequest(new { status = false, message = "Email already confirmed." });
                if (foundUser.clientURI != token) return BadRequest(new { status = false, message = "Invalid token." });
                else foundUser.isConfirmed = true;
            }
            _db.SaveChanges();
            return new JsonResult(new { message = "Email confirmed" });

        }

        [AllowAnonymous]
        [Route("login")]
        [HttpPost]
        public IActionResult Login([FromBody] LoginPayload loginPayload)
        {
            try
            {
                MailAddress m = new MailAddress(loginPayload.Email);
            }
            catch (FormatException)
            {
                return new JsonResult(new { status = "false", message = "email format" });
            }
            var foundUser = _db.Users
               .SingleOrDefault(u => u.Email == loginPayload.Email);

            if (foundUser != null)
            {
                if (foundUser.isConfirmed == false) return BadRequest(new { status = false, message = "Email address not confirmed" });
                if (BCrypt.Net.BCrypt.Verify(loginPayload.Password, foundUser.PasswordHash))
                {
                    var tokenString = GenerateJSONWebToken(foundUser);

                    return new JsonResult(new
                    {
                        status = "true",
                        userID = foundUser.Id.ToString(),
                        token = tokenString

                    });
                }
                return BadRequest(new { status = false, message = "Wrong password or email " });
            }
            else
            {
                return BadRequest(new { status = false, message = "No user with this email found" });
            }

        }

        [Authorize(Roles = Role.Admin)]
        [HttpPatch("changePassword")]
        public IActionResult changePassword(string email, string newPassword)
        {
                var foundUser = _db.Users
                .Where(u => u.Email == email).SingleOrDefault();
            if (foundUser == null) return BadRequest(new { status = false, message = "User not found. Wrong email." });

                foundUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);

                _db.SaveChanges();

                return new JsonResult(new { status = "Password change successfully." });
        }

        [Authorize]
        [HttpPatch("changeUserPassword")]
        public IActionResult changeUserPassword(string newPassword, string oldPassword)
        {
            if(newPassword.Length < 8 || oldPassword.Length < 8)
            {
                return BadRequest(new
                {
                    status = false,
                    message = "Old or new password have less than 8 characters."
                });
            }
            if(newPassword == oldPassword)
            {
                return BadRequest(new
                {
                    status = false,
                    message = "The old and new password can't be the same."
                });
            }
            var foundUser = _db.Users
               .Where(u => u.Id == getUserId()).SingleOrDefault();

            if (!BCrypt.Net.BCrypt.Verify(oldPassword,foundUser.PasswordHash)) return BadRequest(new
            {
                status = false, message = "Old password is incorrect."
            });
            foundUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);

            _db.SaveChanges();

            return new JsonResult(new { status = true });

        }

        [Authorize]
        [HttpPatch("changeUserEmail")]
        public IActionResult changeUserEmail(string newEmail, string password)
        {
            var foundUser = _db.Users
               .SingleOrDefault(u => u.Id == getUserId());
            if (foundUser.PasswordHash != BCrypt.Net.BCrypt.HashPassword(password)) return BadRequest(new
            {
                status = false,
                message = "Incorrect password"
            });
            foundUser.Email = newEmail;

            _db.SaveChanges();

            return new JsonResult(new { status = "Success" });

        }
        [Authorize]
        [HttpPost("addPhoto")]
        public async Task<IActionResult> addPhoto()
        {
            IFormFile file = Request.Form.Files[0];
            string systemFileName = getUserId().ToString();
            string blobstorageconnection = _config["ConnectionStrings:AzureConnectionString"];
            CloudStorageAccount cloudStorageAccount = CloudStorageAccount.Parse(blobstorageconnection);
            CloudBlobClient blobClient = cloudStorageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference("firstcontainer");
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(systemFileName);
            await using (var data = file.OpenReadStream())
            {
                await blockBlob.UploadFromStreamAsync(data);
            }
            var blobUrl = blockBlob.Uri.AbsoluteUri;
            var photo = new ProfilePicture();
            photo.imgLink = blobUrl;
            photo.UserID = getUserId();
            photo.user = _db.Users.Where(u => u.Id == getUserId()).SingleOrDefault();

            _db.ProfilePictures.Add(photo);
            _db.SaveChanges();
            return new JsonResult(new { status = "ok", uri = blobUrl });



        }

        [HttpGet("getUser")]
        public IActionResult getUser(long UserID)
        {
            var foundUser = _db.Users.Where(u => u.Id == UserID).SingleOrDefault();
            var user = new User();
            user.Firstname = foundUser.Firstname;
            user.Lastname = foundUser.Lastname;
            user.Phonenumber = foundUser.Phonenumber;
            user.companyID = foundUser.companyID;
            user.branchID = foundUser.branchID;
            return new JsonResult(new { user = user });
        }

        [HttpGet("getProfilePicture")]
        public IActionResult getProfilePicture(long UserID)
        {
            try
            {
                var found =
                _db.ProfilePictures
                .Where(photo => photo.UserID == UserID)
                .SingleOrDefault();
                if(found == null) return new JsonResult(new { imgLink = "https://carsalewebappstorage.blob.core.windows.net/firstcontainer/no-photo-available-icon-20.jpeg" });
                return new JsonResult(new { imgLink = found.imgLink });

            }
            catch (Exception)
            {
                return new JsonResult(new { status = "false", message = "Photo not found" });
            }
        }

        [HttpGet("forgotPassword")]
        public async Task<IActionResult> forgotPassword(string email)
        {
            var foundUser = _db.Users.Where(u => u.Email == email).SingleOrDefault();
            if (foundUser == null) return BadRequest(new { message = "No user found with the email address " + email });
            if (foundUser.isConfirmed == false) return BadRequest(new { message = "The account is not active." });
            else
            {
                foundUser.clientURI = GenerateJSONWebToken(foundUser);
                _db.SaveChanges();
                using var message = new MimeMessage();
                message.From.Add(new MailboxAddress(
                    "Worldwide Cars Trade",
                    "ww.cars.trade@gmail.com"
                ));
                message.To.Add(new MailboxAddress(
                    foundUser.Firstname + " " + foundUser.Lastname,
                    foundUser.Email
                ));
                message.Subject = "Password Reset";
                var bodyBuilder = new BodyBuilder
                {
                    TextBody = "Hello, Please use the link below to reset your password: \n" +
                    "http://localhost:4200/reset?token=" + foundUser.clientURI + "&email=" + foundUser.Email,
                };
                message.Body = bodyBuilder.ToMessageBody();

                using var client = new MailKit.Net.Smtp.SmtpClient();
                // SecureSocketOptions.StartTls force a secure connection over TLS
                await client.ConnectAsync("smtp.sendgrid.net", 587, SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(
                    userName: "apikey", // the userName is the exact string "apikey" and not the API key itself.
                    password: "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" // password is the API key
                );
                await client.SendAsync(message);

                await client.DisconnectAsync(true);

                return new JsonResult(new { message = "An email to reset the password was sent to: " + email });
            }
        }

        [HttpPatch("resetPassword")]
        public IActionResult resetPassword(string email, string token, string newPassword)
        {
            var foundUser = _db.Users.Where(u => u.Email == email).SingleOrDefault();
            if (foundUser.clientURI != token) return BadRequest(new { message = "Invalid token" });
            else
            {
                foundUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
                foundUser.clientURI = null;
                _db.SaveChanges();
            }
            return new JsonResult(new { message = "Password Sucessfully Changed!", status = true });
        }

        [HttpGet("getMyProfilePicture")]
        public IActionResult getMyProfilePicture()
        {
            try
            {
                var found =
                _db.ProfilePictures
                .Where(photo => getUserId() == photo.user.Id)
                .Single();
                return new JsonResult(new { imgLink = found.imgLink });

            }
            catch (Exception)
            {
                return new JsonResult(new { status = "false", message = "Photo not found", imgLink = "https://carsalewebappstorage.blob.core.windows.net/firstcontainer/no-photo-available-icon-20.jpeg" });
            }
        }
        [Authorize]
        [HttpDelete("deleteProfilePicture")]
        public IActionResult deleteProfilePicture()
        {
            var found =
                _db.ProfilePictures
                .Where(photo => getUserId() == photo.UserID)
                .Single();
            if (found.UserID != getUserId())
            {
                return new JsonResult(new { status = "You can't delete anotherone's profile picture.", ok = "false" });
            }

            string blobstorageconnection = _config["ConnectionStrings:AzureConnectionString"];
            CloudStorageAccount cloudStorageAccount = CloudStorageAccount.Parse(blobstorageconnection);
            CloudBlobClient blobClient = cloudStorageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference("firstcontainer");
            var blob = container.GetBlockBlobReference(getUserId().ToString());
            blob.DeleteIfExistsAsync();

            _db.ProfilePictures.Remove(found);
            _db.SaveChanges();


            return new JsonResult(new { status = "Success", ok = "true" });

        }

        [Authorize]
        [HttpGet("getName")]
        public IActionResult getName()
        {
            var foundUser = _db.Users.Where(u => u.Id == getUserId()).SingleOrDefault();
            return new JsonResult(new { name = foundUser.Lastname + " " + foundUser.Firstname });
        }

        private string GenerateJSONWebToken(User user)
        {

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[] {
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("Role", user.Role),
                new Claim("ID", user.Id.ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                };

            var token = new JwtSecurityToken(_config["Jwt:Issuer"],
              _config["Jwt:Issuer"],
              claims,
              expires: DateTime.Now.AddDays(30),
              signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

    }

}
