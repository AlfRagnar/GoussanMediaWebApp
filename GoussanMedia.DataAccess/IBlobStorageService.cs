using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Components.Forms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace GoussanMedia.DataAccess
{
    public interface IBlobStorageService
    {
        Task<Uri> UploadFileToStorage(Stream stream, string container, string fileName);

        Task<BlobProperties> GetBlobPropertiesAsync(BlobClient blob);

        IAsyncEnumerable<Page<BlobHierarchyItem>> ListBlobsPublic(BlobContainerClient blobContainerClient, int? segmentSize);

        Task<BlobContainerClient> GetContainer(string name);

        Task<string> UploadVideo(IBrowserFile file, string videoName, long maxFileSize = 52428800);
        BlobClient RetrieveBlobAsync(string id);
        Response<bool> DeleteVideo(string videoName);
    }
}