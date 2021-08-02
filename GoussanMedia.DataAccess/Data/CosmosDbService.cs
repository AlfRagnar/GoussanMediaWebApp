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

        public CosmosDbService(CosmosClient dbClient, string databaseName) : base()
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

        public async Task AddItemAsync(ToDoList item, Container container)
        {
            try
            {
                await container.CreateItemAsync(item, new PartitionKey(item.Id));
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

        public async Task DeleteItemAsync(string id, string userId, Container container)
        {
            try
            {
                await container.DeleteItemAsync<ToDoList>(id, new PartitionKey(userId));
            }
            catch (CosmosException)
            {
                throw;
            }
        }

       

        public async Task<ToDoList> GetItemAsync(string id, Container container)
        {
            try
            {
                ItemResponse<ToDoList> response = await container.ReadItemAsync<ToDoList>(id, new PartitionKey(id));
                return response.Resource;
            }
            catch (CosmosException)
            {
                return null;
            }
        }


        public async Task<List<ToDoList>> GetMyItems(string userId, Container container)
        {
            List<ToDoList> MyList = new();
            using (FeedIterator<ToDoList> results = container.GetItemQueryIterator<ToDoList>(
                queryDefinition: null,
                requestOptions: new QueryRequestOptions()
                {
                    PartitionKey = new PartitionKey(userId)
                }))
            {
                while (results.HasMoreResults)
                {
                    FeedResponse<ToDoList> response = await results.ReadNextAsync();
                    if (response.Diagnostics != null)
                    {
                        Console.WriteLine($" Diagnostics {response.Diagnostics}");
                    }

                    MyList.AddRange(response);
                }
            }
            return MyList;
        }

        public async Task<IEnumerable<ToDoList>> GetItemsAsync(string queryString, Container container)
        {
            try
            {
                FeedIterator<ToDoList> query = container.GetItemQueryIterator<ToDoList>(new QueryDefinition(queryString));
                List<ToDoList> results = new();
                while (query.HasMoreResults)
                {
                    FeedResponse<ToDoList> response = await query.ReadNextAsync();

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

        public async Task UpdateItem(ToDoList item, Container container)
        {
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

        public void Dispose()
        {
            _client.Dispose();
            GC.SuppressFinalize(this);
        }

        public Task AddVideoAsync(Videos video, Container container)
        {
            throw new NotImplementedException();
        }
    }
}