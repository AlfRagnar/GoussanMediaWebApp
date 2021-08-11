using GoussanMedia.Domain.Models;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Azure.Management.Media.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GoussanMedia.DataAccess
{
    public interface IAzMediaService
    {
        Task<Videos> CreateAsset(IBrowserFile fileToUpload, Videos videos, long maxFileSize = 52428800);

        Task<StreamingLocator> CreateStreamingLocatorAsync(string assetName, string locatorName = null);

        Task<StreamingEndpoint> EnsureStreamingEndpoint(string Endpoint = "default");

        Task<Transform> GetOrCreateTransformAsync(string transformName = "GoussanAdaptiveStreamingPreset");

        Task<IList<string>> GetStreamingUrlsAsync(string locatorName);
    }
}