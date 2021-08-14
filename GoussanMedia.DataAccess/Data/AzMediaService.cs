using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using GoussanMedia.Domain;
using GoussanMedia.Domain.Models;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Azure.Management.Media;
using Microsoft.Azure.Management.Media.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GoussanMedia.DataAccess.Data
{
    public class AzMediaService : IAzMediaService
    {
        private readonly AzureMediaServicesClient _azMediaServices;
        private readonly string resourceGroupName = Config.ResourceGroup;
        private readonly string accountName = Config.AccountName;

        public AzMediaService(AzureMediaServicesClient azureMediaServicesClient)
        {
            _azMediaServices = azureMediaServicesClient;
        }

        public async Task<Videos> CreateAsset(IBrowserFile fileToUpload, Videos videos, long maxFileSize = 1024 * 1024 * 50)
        {
            // Create the Asset in Azure Media Service
            Asset asset = await InternalCreate(videos.Id);

            // Get the Shared Access Signature for the Asset Container matching the Video ID
            AssetContainerSas response = await ListContainers(asset.Name);

            // Fetch the first available SAS Uri available
            Uri sasUri = new(response.AssetContainerSasUrls.First());

            // Upload the file to Azure Blob Storage with the SAS URI from Azure Media Services
            _ = await UploadFile(fileToUpload, asset.Name, sasUri, maxFileSize);

            Asset outputAsset = await CreateOutputAsset(asset.Name);

            _ = await SubmitJobAsync(asset.Name, outputAsset.Name);

            StreamingLocator locator = await CreateStreamingLocatorAsync(outputAsset.Name);
            videos.Locator = locator.Name;
            videos.OutputAsset = outputAsset.Name;
            return videos;
        }

        private static async Task<Response<BlobContentInfo>> UploadFile(IBrowserFile file, string fileName, Uri uri, long maxFileSize)
        {
            try
            {
                Response<BlobContentInfo> response;
                // Create the blob container client using the SAS URI
                BlobContainerClient blobContainerClient = new(uri);
                // Create the blob client
                BlobClient blob = blobContainerClient.GetBlobClient(fileName);
                // Read the filestream
                using (var fs = file.OpenReadStream(maxFileSize))
                {
                    // Upload data to Azure Blob Storage
                    response = await blob.UploadAsync(fs);
                }
                return response;
            }
            catch (RequestFailedException)
            {
                return null;
            }
        }

        // Operations to create the asset
        private async Task<Asset> InternalCreate(string ID)
        {
            Asset asset = await _azMediaServices.Assets.CreateOrUpdateAsync(resourceGroupName, accountName, ID, new Asset());
            return asset;
        }

        // Operations to Get a list of Containers matching the ID passed
        private async Task<AssetContainerSas> ListContainers(string ID)
        {
            AssetContainerSas response = await _azMediaServices.Assets.ListContainerSasAsync(resourceGroupName, accountName, ID,
                permissions: AssetContainerPermission.ReadWrite,
                expiryTime: DateTime.UtcNow.AddHours(4).ToUniversalTime());

            return response;
        }

        private async Task<Asset> CreateOutputAsset(string ID)
        {
            // Check if asset already exist
            Asset outputAsset = await _azMediaServices.Assets.GetAsync(resourceGroupName, accountName, ID);
            Asset asset = new();
            string outputAssetName = ID;

            if (outputAsset != null)
            {
                // Name collision! time to create a new unique name for the asset
                string unique = $"-Encoded";
                outputAssetName += unique;
            }
            // Create the Asset for the encoding Job to be written to
            Asset output = await _azMediaServices.Assets.CreateOrUpdateAsync(resourceGroupName, accountName, outputAssetName, asset);
            return output;
        }

        // Creates a Job with information about how to Encode the Asset
        private async Task<Job> SubmitJobAsync(string inputAsset, string outputAsset, string transformName = "GoussanAdaptiveStreamingPreset", string jobName = "GoussanEncoding")
        {
            jobName += "-" + inputAsset;

            // Create the Input object
            JobInputAsset jobInput = new(inputAsset);
            // Create the Output object
            JobOutput[] jobOutputs =
            {
                new JobOutputAsset(outputAsset)
            };

            // Check if job already exists
            var getjob = await _azMediaServices.Jobs.GetAsync(resourceGroupName, accountName, transformName, jobName);

            if (getjob == null)
            {
                // if job doesn't exist, then we create a new job
                Job job = await _azMediaServices.Jobs.CreateAsync(resourceGroupName, accountName, transformName, jobName, new Job
                {
                    Input = jobInput,
                    Outputs = jobOutputs
                });
                // Return the job
                return job;
            }
            // Return the job
            return getjob;
        }

        // Recipe or Encoding of the content in Media Services
        public async Task<Transform> GetOrCreateTransformAsync(string transformName = "GoussanAdaptiveStreamingPreset")
        {
            // Does a Transform already exist with the desired name?
            Transform transform = await _azMediaServices.Transforms.GetAsync(resourceGroupName, accountName, transformName);

            if (transform == null)
            {
                // Specify what I need it to produce as an output
                TransformOutput[] output = new TransformOutput[]
                {
                    new TransformOutput
                    {
                        // Set the preset
                        Preset = new BuiltInStandardEncoderPreset()
                        {
                            PresetName = EncoderNamedPreset.AdaptiveStreaming
                        }
                    }
                };

                // Create the Transform with the output defined above
                transform = await _azMediaServices.Transforms.CreateOrUpdateAsync(resourceGroupName, accountName, transformName, output);
            }
            return transform;
        }

        // Need to create a Streaming Locator for the specified asset to be available for playback for clients.
        public async Task<StreamingLocator> CreateStreamingLocatorAsync(string assetName, string locatorName = null)
        {
            if (string.IsNullOrEmpty(locatorName))
            {
                var uid = Regex.Replace(Convert.ToBase64String(Guid.NewGuid().ToByteArray()), "[/+=]", "");
                locatorName = uid;
            }
            StreamingLocator locator = await _azMediaServices.StreamingLocators.CreateAsync(resourceGroupName, accountName, locatorName, new StreamingLocator
            {
                AssetName = assetName,
                StreamingPolicyName = PredefinedStreamingPolicy.ClearStreamingOnly
            });
            return locator;
        }

        // Builds the streaming URLs available
        public async Task<IList<string>> GetStreamingURL(string locatorName)
        {
            try
            {
                IList<string> streamingUrls = new List<string>();
                StreamingEndpoint endpoint = await EnsureStreamingEndpoint();

                ListPathsResponse paths = await _azMediaServices.StreamingLocators.ListPathsAsync(resourceGroupName, accountName, locatorName);

                foreach (StreamingPath path in paths.StreamingPaths)
                {
                    UriBuilder uriBuilder = new()
                    {
                        Scheme = "https",
                        Host = endpoint.HostName,
                        Path = path.Paths[0]
                    };
                    streamingUrls.Add(uriBuilder.ToString());
                }

                return streamingUrls;
            }
            catch (Exception)
            {
                return null;
            }
        }

        // Ensure that streaming endpoint is online and running
        public async Task<StreamingEndpoint> EnsureStreamingEndpoint(string Endpoint = "default")
        {
            StreamingEndpoint streamingEndpoint = await _azMediaServices.StreamingEndpoints.GetAsync(resourceGroupName, accountName, Endpoint);

            if (streamingEndpoint != null)
            {
                if (streamingEndpoint.ResourceState != StreamingEndpointResourceState.Running)
                {
                    await _azMediaServices.StreamingEndpoints.StartAsync(resourceGroupName, accountName, Endpoint);
                }
            }
            return streamingEndpoint;
        }

        // Download the results from specified output asset, JUST TEST not going for local file storage
        public async Task DownloadOutputAssetAsync(string assetName)
        {
            var currentDir = Directory.GetCurrentDirectory();

            // Use Media service and Storage APIs to download the output file to a local folder
            AssetContainerSas assetContainerSas = _azMediaServices.Assets.ListContainerSas(
                resourceGroupName,
                accountName,
                assetName,
                permissions: AssetContainerPermission.Read,
                expiryTime: DateTime.UtcNow.AddHours(1).ToUniversalTime());

            Uri containerSasUrl = new(assetContainerSas.AssetContainerSasUrls.FirstOrDefault());
            BlobContainerClient container = new(containerSasUrl);

            string directory = Path.Combine(currentDir, assetName);
            Directory.CreateDirectory(directory);
            Trace.WriteLine("Downloading resulst to {0}", directory);

            string continuationToken = null;

            // Call the listing operation and enumerate the result segment
            // When the continuation token is empty, the last segment has been returned and execution can exit the loop
            do
            {
                var resultSegment = container.GetBlobs().AsPages(continuationToken);

                foreach (Azure.Page<BlobItem> blobPage in resultSegment)
                {
                    foreach (BlobItem blobItem in blobPage.Values)
                    {
                        var blobClient = container.GetBlobClient(blobItem.Name);
                        string fileName = Path.Combine(directory, blobItem.Name);
                        await blobClient.DownloadToAsync(fileName);
                    }

                    // Get the continuation token and loop until it is empty
                    continuationToken = blobPage.ContinuationToken;
                }
            } while (continuationToken != "");

            Trace.WriteLine("Download complete.");
        }

        // Fetch the stream URLs currently available
        public async Task<IList<string>> OldGetStreamingUrlsAsync(string locatorName)
        {
            const string DefaultStreamingEndpointName = "default";

            IList<string> streamingUrls = new List<string>();

            StreamingEndpoint streamingEndpoint = await _azMediaServices.StreamingEndpoints.GetAsync(resourceGroupName, accountName, DefaultStreamingEndpointName);

            if (streamingEndpoint != null)
            {
                if (streamingEndpoint.ResourceState != StreamingEndpointResourceState.Running)
                {
                    await _azMediaServices.StreamingEndpoints.StartAsync(resourceGroupName, accountName, DefaultStreamingEndpointName);
                }
            }

            ListPathsResponse paths = await _azMediaServices.StreamingLocators.ListPathsAsync(resourceGroupName, accountName, locatorName);

            foreach (StreamingPath path in paths.StreamingPaths)
            {
                UriBuilder uriBuilder = new()
                {
                    Scheme = "https",
                    Host = streamingEndpoint.HostName,
                    Path = path.Paths[0]
                };
                streamingUrls.Add(uriBuilder.ToString());
            }

            return streamingUrls;
        }

        // Run to clean up resources created if/when needed
        public async Task CleanUpAsync(string transformName, string jobName, List<string> assetNames, string contentKeyPolicyName = null)
        {
            await _azMediaServices.Jobs.DeleteAsync(resourceGroupName, accountName, transformName, jobName);

            foreach (var assetName in assetNames)
            {
                await _azMediaServices.Assets.DeleteAsync(resourceGroupName, accountName, assetName);
            }

            if (contentKeyPolicyName != null)
            {
                _azMediaServices.ContentKeyPolicies.Delete(resourceGroupName, accountName, contentKeyPolicyName);
            }
        }
    }
}