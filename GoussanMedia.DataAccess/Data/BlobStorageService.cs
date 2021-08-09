using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace GoussanMedia.DataAccess.Data
{
    public class BlobStorageService : IBlobStorageService
    {
        private readonly BlobServiceClient _blobServiceClient;

        public BlobStorageService(BlobServiceClient blobServiceClient)
        {
            _blobServiceClient = blobServiceClient;
        }

        public async Task<BlobContainerClient> GetContainer(string name = "Videos")
        {
            try
            {
                BlobContainerClient container = await _blobServiceClient.CreateBlobContainerAsync(name);
                if (await container.ExistsAsync())
                {
                    return container;
                }
            }
            catch (RequestFailedException)
            {
            }
            return null;
        }

        public static IAsyncEnumerable<Page<BlobHierarchyItem>> ListBlobsPublic(BlobContainerClient blobContainerClient, int? segmentSize)
        {
            try
            {
                var resultSegment = blobContainerClient.GetBlobsByHierarchyAsync(prefix: "public").AsPages(default, segmentSize);
                return resultSegment;
            }
            catch (RequestFailedException)
            {
                return null;
            }
        }

        public static async Task<BlobProperties> GetBlobPropertiesAsync(BlobClient blob)
        {
            try
            {
                BlobProperties blobProperties = await blob.GetPropertiesAsync();
                return blobProperties;
            }
            catch (RequestFailedException)
            {
                return null;
            }
        }

        public async Task<Uri> UploadFileToStorage(Stream stream, string container, string fileName)
        {
            string newFileName = Guid.Parse(fileName).ToString();
            Uri blobUri = new(_blobServiceClient.Uri + container + newFileName);
            BlobClient blobClient = new(blobUri);

            await blobClient.UploadAsync(stream);
            return blobUri;
        }

        public Task Save(Stream fileStream, string name)
        {
            throw new NotImplementedException();
        }
    }
}