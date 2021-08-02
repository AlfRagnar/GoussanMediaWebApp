using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage.Sas;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace GoussanMedia.DataAccess.Data
{
    public class BlobStorageService
    {
        private readonly string containerName = "Videos";
        private readonly BlobServiceClient _blobServiceClient;

        public BlobStorageService(BlobServiceClient blobServiceClient) : base()
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

        private static Uri GetServiceSasUriForContainer(BlobContainerClient containerClient, string storedPolicyName = null)
        {
            if (containerClient.CanGenerateSasUri)
            {
                BlobSasBuilder sasBuilder = new()
                {
                    BlobContainerName = containerClient.Name,
                    Resource = "c"
                };

                if (storedPolicyName == null)
                {
                    sasBuilder.ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(5);
                    sasBuilder.SetPermissions(BlobContainerSasPermissions.Read);
                }
                else if (storedPolicyName.ToLower().Equals("create"))
                {
                    sasBuilder.ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(10);
                    sasBuilder.SetPermissions(BlobContainerSasPermissions.Create);
                }
                else
                {
                    sasBuilder.Identifier = storedPolicyName;
                }

                Uri sasUri = containerClient.GenerateSasUri(sasBuilder);
                return sasUri;
            }
            else
            {
                return null;
            }
        }

        private static Uri GetServiceSasUriForBlob(BlobClient blobClient, string storedPolicyName = null)
        {
            if (blobClient.CanGenerateSasUri)
            {
                BlobSasBuilder sasBuilder = new()
                {
                    BlobContainerName = blobClient.GetParentBlobContainerClient().Name,
                    BlobName = blobClient.Name,
                    Resource = "b"
                };

                if (storedPolicyName == null)
                {
                    sasBuilder.ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(5);
                    sasBuilder.SetPermissions(BlobSasPermissions.Read | BlobSasPermissions.Write);
                }
                else if (storedPolicyName.ToLower().Equals("create"))
                {
                    sasBuilder.ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(10);
                    sasBuilder.SetPermissions(BlobSasPermissions.Create);
                }
                else
                {
                    sasBuilder.Identifier = storedPolicyName;
                }

                Uri sasUri = blobClient.GenerateSasUri(sasBuilder);
                return sasUri;
            }
            else
            {
                return null;
            }
        }
    }
}