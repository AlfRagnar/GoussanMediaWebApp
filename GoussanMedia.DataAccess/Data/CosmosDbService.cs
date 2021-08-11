using GoussanMedia.Domain;
using GoussanMedia.Domain.Models;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GoussanMedia.DataAccess.Data
{
    public class CosmosDbService : ICosmosDbService
    {
        private readonly CosmosClient _client;
        private readonly Database _dbClient;

        public CosmosDbService(CosmosClient dbClient, string databaseName)
        {
            _client = dbClient;
            _dbClient = dbClient.GetDatabase(databaseName);
        }

        // Set the container for when the service is ran
        public Container GetContainer(string containerName)
        {
            try
            {
                Container container = _dbClient.GetContainer(containerName);
                return container;
            }
            catch (CosmosException)
            {
                throw;
            }
        }

        public async Task<ContainerResponse> CheckContainer(string containerName, string partitionKeyPath)
        {
            try
            {
                ContainerResponse containerResponse = await _dbClient.CreateContainerIfNotExistsAsync(
               id: containerName,
               partitionKeyPath: partitionKeyPath,
               throughput: 400);
                return containerResponse;
            }
            catch (CosmosException)
            {
                throw;
            }
        }

        public async Task AddVideo(Videos videos, Container container)
        {
            try
            {
                await container.CreateItemAsync(videos, new PartitionKey(videos.Id));
            }
            catch (CosmosException)
            {
                throw;
            }
        }

        public async Task<Videos> GetVideoAsync(string id, Container container)
        {
            try
            {
                ItemResponse<Videos> response = await container.ReadItemAsync<Videos>(id, new PartitionKey(id));
                return response.Resource;
            }
            catch (CosmosException)
            {
                return null;
            }
        }

        public async Task<IEnumerable<Videos>> GetVideos(Container container)
        {
            try
            {
                FeedIterator<Videos> documentsQuery = container.GetItemQueryIterator<Videos>(new QueryDefinition($"Select * from {container.Id}"));
                List<Videos> results = new();
                while (documentsQuery.HasMoreResults)
                {
                    FeedResponse<Videos> response = await documentsQuery.ReadNextAsync();
                    results.AddRange(response.ToList());
                }
                return results;
            }
            catch (CosmosException)
            {
                throw;
            }
        }

        public async Task<IEnumerable<Videos>> GetUploadsAsync(string queryString, Container container)
        {
            try
            {
                IOrderedQueryable<Videos> query = container.GetItemLinqQueryable<Videos>();
                FeedIterator<Videos> iterator = query.ToFeedIterator();
                FeedResponse<Videos> results = await iterator.ReadNextAsync();
                return results;
            }
            catch (CosmosException)
            {
                throw;
            }
        }

        public async Task UpdateVideo(Videos item, string containerName)
        {
            Container container;
            if (string.IsNullOrEmpty(containerName))
            {
                container = GetContainer(Config.CosmosVideos);
            } else
            {
                container = GetContainer(containerName);
            }
            try
            {
                await container.UpsertItemAsync(item, new PartitionKey(item.Id));
            }
            catch (CosmosException)
            {
                throw;
            }
        }

        public async Task<DatabaseResponse> CheckDatabase(string database)
        {
            try
            {
                DatabaseResponse databaseResponse = await _client.CreateDatabaseIfNotExistsAsync(database);
                return databaseResponse;
            }
            catch (CosmosException)
            {
                throw;
            }
        }

        public Task ListContainersInDatabase()
        {
            throw new NotImplementedException();
        }

        public Task AddVideoAsync(Videos video, Container container)
        {
            throw new NotImplementedException();
        }
    }
}