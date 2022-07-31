using System;
using System.IdentityModel.Tokens.Jwt;
using Backend.Controllers;
using Backend.Entities;
using System.Security.Claims;
using System.Text;
using Backend.DTO;
using Backend.Entities.Models;
using Backend.Payloads;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MimeKit;

namespace Backend.Services;

public class AccountService : IAccountService
{
    public IConfiguration _config { get; }
    private IMailService _mailService;
    private IPhotoService _photoService;
    private IRepositoryManager _repositoryManager;
    private readonly BackendContext _db;

    public AccountService(BackendContext db, IConfiguration configuration, IMailService mailService, IPhotoService photoService, IRepositoryManager repositoryManager)
    {
        _config = configuration;
        _db = db;
        _mailService = mailService;
        _photoService = photoService;
        _repositoryManager = repositoryManager;
    }

    public string GenerateJSONWebToken(User user)
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

    public async void Register(RegisterPayload registerPayload)
    {
        var existingUserWithMail = _repositoryManager.getUserByEmail(registerPayload.Email);
        if (existingUserWithMail != null) throw new Exception("Email already exists.");
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
            companyID = null,
            branchID = null
        };
        userToCreate.clientURI = GenerateJSONWebToken(userToCreate);
        _repositoryManager.addUser(userToCreate);
        
        var bodyBuilder = new BodyBuilder
        {
            TextBody = "Hello, welcome to WorldWide Cars Trade. Please use the link below to activate your account. \n" +
                       "http://localhost:4200/confirm?token=" + userToCreate.clientURI + "&email=" + userToCreate.Email,
        };

        await _mailService.sendEmail(userToCreate.Firstname + " " + userToCreate.Lastname, userToCreate.Email,
            "Email address Activation", bodyBuilder);

    }

    public long confirmEmail(string email, string token)
    {
        var foundUser = _repositoryManager.getUserByEmail(email);
        if (foundUser == null) throw new Exception("User not found.");
        if (foundUser.isConfirmed == true) throw new Exception("Email already confirmed.");
        if (foundUser.clientURI != token) throw new Exception("Invalid token.");
        else foundUser.isConfirmed = true;
        _db.SaveChanges();
        return 1;
    }

    public string login(LoginPayload loginPayload)
    {
        var foundUser = _repositoryManager.getUserByEmail(loginPayload.Email);
        if (foundUser == null) throw new Exception("User not found.");
        if (foundUser != null)
        {
            if (foundUser.isConfirmed == false) throw new Exception("Account not confirmed.");
            if (BCrypt.Net.BCrypt.Verify(loginPayload.Password, foundUser.PasswordHash))
            {
                var tokenString = GenerateJSONWebToken(foundUser);
                return tokenString;
            }
            throw new Exception("Wrong email or password.");
        }

        return "";
    }

    public void changePassword(string email, string newPassword)
    {
        var foundUser = _repositoryManager.getUserByEmail(email);
        if (foundUser == null) throw new Exception("User not found.");
        foundUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        _db.SaveChanges();
    }

    public void changeUserPassword(string newPassword, string oldPassword, long userID)
    {
        if (newPassword == oldPassword) throw new Exception("The old and new password can't be the same.");
        var foundUser = _repositoryManager.getUserById(userID);
        if (!BCrypt.Net.BCrypt.Verify(oldPassword, foundUser.PasswordHash)) throw new Exception("Old password is incorrect.");
        foundUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        _db.SaveChanges();
    }

    public void changeUserEmail(string newEmail, string password, long userID)
    {
        var foundUser = _repositoryManager.getUserById(userID);
        if (foundUser.PasswordHash != BCrypt.Net.BCrypt.HashPassword(password)) throw new Exception("Incorrect password");
        foundUser.Email = newEmail;
        _db.SaveChanges();
    }

    public string addProfilePhoto(long userID, IFormFile file)
    {
        var image = _photoService.addPhoto(file).Result;
        var photo = new ProfilePicture()
        {
            imgLink = image.blobUrl,
            UserID = userID
        };
        _repositoryManager.addProfilePicture(photo);
        return image.blobUrl;
    }

    public string getName(long userID)
    {
        var foundUser = _repositoryManager.getUserById(userID);
        return foundUser.Firstname + " " + foundUser.Lastname;
    }

    public UserDTO getUser(long userID)
    {
        var foundUser = _repositoryManager.getUserById(userID);
        var user = new UserDTO()
        {
            Firstname = foundUser.Firstname,
            Lastname = foundUser.Lastname,
            Phonenumber = foundUser.Phonenumber,
            companyID = foundUser.companyID.GetValueOrDefault(),
            branchID = foundUser.branchID.GetValueOrDefault()
        };
        return user;
    }
    
    public void deleteProfilePicture(long userID)
    {
        var found = _repositoryManager.getProfilePictureById(userID);
        _photoService.deletePhoto(userID.ToString());
        _repositoryManager.removeProfilePicture(found);
    }

    public async void forgotPassword(string email)
    {
        var foundUser = _repositoryManager.getUserByEmail(email);
        if (foundUser == null) throw new Exception("No user found with the email address " + email);
        if (foundUser.isConfirmed == false) throw new Exception("The account is not active.");
        foundUser.clientURI = GenerateJSONWebToken(foundUser);
        _db.SaveChanges();
        var bodyBuilder = new BodyBuilder
        {
            TextBody = "Hello, Please use the link below to reset your password: \n" +
                       "http://localhost:4200/reset?token=" + foundUser.clientURI + "&email=" + foundUser.Email,
        };
        await _mailService.sendEmail(foundUser.Firstname + " " + foundUser.Lastname, foundUser.Email, "Password Reset",
            bodyBuilder);
    }

    public void resetPassword(string email, string token, string newPassword)
    {
        var foundUser = _repositoryManager.getUserByEmail(email);
        if (foundUser.clientURI != token) throw new Exception("Invalid token.");
        foundUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        foundUser.clientURI = null;
        _db.SaveChanges();
    }
}