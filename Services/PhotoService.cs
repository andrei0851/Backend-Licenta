using System;
using System.Threading.Tasks;
using Backend.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Org.BouncyCastle.Asn1.Ocsp;

namespace Backend.Services;

public class PhotoService : IPhotoService
{
    private IConfiguration _config { get; }

    public PhotoService(IConfiguration configuration)
    {
        _config = configuration;
    }

    public async Task<ImageDTO> addPhoto(IFormFile file)
    {
        string systemFileName = Guid.NewGuid().ToString();
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

        var imageDTO = new ImageDTO
        {
            blobUrl = blobUrl,
            systemFileName = systemFileName
        };

        return imageDTO;
    }
    
    public void deletePhoto(string filename)
    {
        string blobstorageconnection = _config["ConnectionStrings:AzureConnectionString"];
        CloudStorageAccount cloudStorageAccount = CloudStorageAccount.Parse(blobstorageconnection);
        CloudBlobClient blobClient = cloudStorageAccount.CreateCloudBlobClient();
        CloudBlobContainer container = blobClient.GetContainerReference("firstcontainer");
        var blob = container.GetBlockBlobReference(filename);
        blob.DeleteIfExistsAsync();
    }
}