
//Using Dependency Injection to Inject the methods for Crud operations on the images(blobs) as per CLEAN 

namespace Red_Mango_API.Services
{
    //Inferface does not have an implementation. We are siply defining functions we're going to have when we do implement IBlobService
    public interface IBlobService
    {
        Task<string> GetBlob(string blobName, string containerName);
        Task<string> UploadBlob(string blobName, string containerName, IFormFile file);
        Task<bool> DeleteBlob(string blobName, string containerName);
    }
}
