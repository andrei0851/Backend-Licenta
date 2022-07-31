using Backend.Controllers;
using Backend.DTO;
using Backend.Entities.Models;
using Backend.Payloads;
using Microsoft.AspNetCore.Http;

namespace Backend.Services;

public interface IAccountService
{
    string GenerateJSONWebToken(User user);
    void Register(RegisterPayload registerPayload);
    long confirmEmail(string email, string token);
    string login(LoginPayload loginPayload);
    void changePassword(string email, string newPassword);
    void changeUserPassword(string newPassword, string oldPassword, long userID);
    void changeUserEmail(string newEmail, string password, long userID);
    string addProfilePhoto(long userID, IFormFile file);
    string getName(long userID);
    UserDTO getUser(long userID);
    void deleteProfilePicture(long userID);
    void forgotPassword(string email);
    void resetPassword(string email, string token, string newPassword);
}