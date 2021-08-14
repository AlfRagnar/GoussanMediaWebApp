using GoussanMedia.Domain.Models;
using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GoussanMedia.DataAccess
{
    public interface ICosmosDbService
    {
        // CRUD OPERATIONS

        Task AddVideoAsync(Videos video, Container container);

        Task AddVideo(Videos videos, Container container);

        Task UpdateVideo(Videos item, string container = null);

        // META CRUD OPERATIONS
        Task<DatabaseResponse> CheckDatabase(string database);

        Task<ContainerResponse> CheckContainer(string containerName, string partitionKeyPath);

        // FETCH OPERATIONS
        Container GetContainer(string containerName);

        Task<Videos> GetVideoAsync(string id, Container container);


        Task<IEnumerable<Videos>> GetVideos(Container container);

        Task<IEnumerable<Videos>> GetUploadsAsync(string queryString, Container container);

        Task ListContainersInDatabase();
        Task<IEnumerable<Videos>> GetStreamingVideosAsync(Container container);
        IEnumerable<Videos> GetStreamingVideos(Container container);
    }
}