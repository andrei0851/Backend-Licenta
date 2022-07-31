using System.Threading.Tasks;
using Backend.DTO;
using Microsoft.AspNetCore.Http;

namespace Backend.Services;

public interface IPhotoService
{
    Task<ImageDTO> addPhoto(IFormFile file);
    void deletePhoto(string filename);
}