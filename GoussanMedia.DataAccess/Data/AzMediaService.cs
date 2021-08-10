using Azure.Storage.Blobs;
using GoussanMedia.Domain;
using GoussanMedia.Domain.Models;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Azure.Management.Media;
using Microsoft.Azure.Management.Media.Models;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace GoussanMedia.DataAccess.Data
{
    public class AzMediaService : IAzMediaService
    {
        private readonly AzureMediaServicesClient _azMediaServices;
        private readonly IBlobStorageService blobStorage;
        private readonly string resourcegroupName = Config.ResourceGroup;
        private readonly string accountName = Config.AccountName;

        public AzMediaService(AzureMediaServicesClient azureMediaServicesClient)
        {
            _azMediaServices = azureMediaServicesClient;
        }

        public async Task<Asset> CreateAsset(IBrowserFile fileToUpload, Videos videos, long maxFileSize = 1024 * 1024 * 50)
        {
            Asset asset = await _azMediaServices.Assets.CreateOrUpdateAsync(resourcegroupName, accountName, videos.Id, new Asset());

            var response = await _azMediaServices.Assets.ListContainerSasAsync(resourcegroupName, accountName, videos.Id, permissions: AssetContainerPermission.ReadWrite, expiryTime: DateTime.UtcNow.AddHours(4).ToUniversalTime());

            var sasUri = new Uri(response.AssetContainerSasUrls.First());

            BlobContainerClient blobContainerClient = new(sasUri);
            BlobClient blob = blobContainerClient.GetBlobClient(videos.Id);
            using (var fs = fileToUpload.OpenReadStream(maxFileSize))
            {
                await blob.UploadAsync(fs);
            }
            return asset;
        }

        public async Task<Asset> CreateoutputAsset(string assetName)
        {
            Asset outputAsset = await _azMediaServices.Assets.GetAsync(resourcegroupName, accountName, assetName);
            Asset asset = new();
            string outputAssetName = assetName;

            if (outputAsset != null)
            {
                // Name collision! time to create a new unique name for the asset
                string unique = $"-{Guid.NewGuid():N}";
                outputAssetName += unique;
            }
            return await _azMediaServices.Assets.CreateOrUpdateAsync(resourcegroupName, accountName, outputAssetName, asset);
        }

        public async Task<Asset> CreateInputAssetAsync(
            string resourceGroupName,
            string accountName,
            string assetName,
            string fileToUpload)
        {
            // In this example, we are assuming that the asset name is unique.
            //
            // If you already have an asset with the desired name, use the Assets.Get method
            // to get the existing asset. In Media Services v3, the Get method on entities returns null
            // if the entity doesn't exist (a case-insensitive check on the name).

            // Call Media Services API to create an Asset.
            // This method creates a container in storage for the Asset.
            // The files (blobs) associated with the asset will be stored in this container.
            Asset asset = await _azMediaServices.Assets.CreateOrUpdateAsync(resourceGroupName, accountName, assetName, new Asset());

            // Use Media Services API to get back a response that contains
            // SAS URL for the Asset container into which to upload blobs.
            // That is where you would specify read-write permissions
            // and the exparation time for the SAS URL.
            var response = await _azMediaServices.Assets
                .ListContainerSasAsync(
                resourceGroupName,
                accountName,
                assetName,
                permissions: AssetContainerPermission.ReadWrite,
                expiryTime: DateTime.UtcNow.AddHours(4).ToUniversalTime());

            var sasUri = new Uri(response.AssetContainerSasUrls.First());

            // Use Storage API to get a reference to the Asset container
            // that was created by calling Asset's CreateOrUpdate method.
            BlobContainerClient container = new(sasUri);

            BlobClient blob = container.GetBlobClient(Path.GetFileName(fileToUpload));

            // Use Strorage API to upload the file into the container in storage.
            await blob.UploadAsync(fileToUpload);

            return asset;
        }

        public async Task<Asset> CreateOutputAssetAsync(string resourceGroupName, string accountName, string assetName)
        {
            // Check if an Asset already exists
            Asset outputAsset = await _azMediaServices.Assets.GetAsync(resourceGroupName, accountName, assetName);
            Asset asset = new();
            string outputAssetName = assetName;

            if (outputAsset != null)
            {
                // Name collision! In order to get the sample to work, let's just go ahead and create a unique asset name
                // Note that the returned Asset can have a different name than the one specified as an input parameter.
                // You may want to update this part to throw an Exception instead, and handle name collisions differently.
                string uniqueness = $"-{Guid.NewGuid():N}";
                outputAssetName += uniqueness;

                Console.WriteLine("Warning – found an existing Asset with name = " + assetName);
                Console.WriteLine("Creating an Asset with this name instead: " + outputAssetName);
            }

            return await _azMediaServices.Assets.CreateOrUpdateAsync(resourceGroupName, accountName, outputAssetName, asset);
        }
    }
}