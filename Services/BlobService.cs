
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace Red_Mango_API.Services
{
    public class BlobService : IBlobService
    {
        //Injecting blob service client 
        private readonly BlobServiceClient _blobClient;

        public BlobService(BlobServiceClient blobClient)
        {
            _blobClient = blobClient;
        }
        //end injection of blob service client

        //Begin implementing IBlobClient to Delete Image(blob)
        public async Task<bool> DeleteBlob(string blobName, string containerName)
        {
            BlobContainerClient blobContainerClient = _blobClient.GetBlobContainerClient(containerName);
            BlobClient blobClient = blobContainerClient.GetBlobClient(blobName);

            return await blobClient.DeleteIfExistsAsync();
        }
        //End implementing IBlobClient to delete Image(blob)


        //Begin implementing IBlobClient to Get Image(blob)
        public Task<string> GetBlob(string blobName, string containerName)
        {
            BlobContainerClient blobContainerClient = _blobClient.GetBlobContainerClient(containerName);
            BlobClient blobClient = blobContainerClient.GetBlobClient(blobName);

            string blobUri = blobClient.Uri.AbsoluteUri;

            return Task.FromResult(blobUri);
        }
        //End implementing IBlobClient to Get Image(blob)


        //Begin implementing IBlobClient to Upload Image(blob)
        public async Task<string> UploadBlob(string blobName, string containerName, IFormFile file)
        {
            BlobContainerClient blobContainerClient = _blobClient.GetBlobContainerClient(containerName);
            BlobClient blobClient = blobContainerClient.GetBlobClient(blobName);

            var httpHeaders = new BlobHttpHeaders()
            {
                ContentType = file.ContentType,
            };

            // Upload the file to the blob storage
            var result = await blobClient.UploadAsync(file.OpenReadStream(), httpHeaders);

            if (result != null)
            {
                // Return the URI of the uploaded blob
                return await GetBlob(blobName, containerName); // Await the result here
            }

            return ""; // Or any appropriate error handling
        }

        //End implementing IBlobClient to Upload Image(blob)

    }
}
