using GoussanMedia.Domain.Models;
using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GoussanMedia.DataAccess
{
    public interface ICosmosDbService : IDisposable
    {
        // CRUD OPERATIONS
        Task AddItemAsync(ToDoList item, Container container);

        Task AddVideoAsync(Videos video, Container container);

        Task AddVideo(Videos videos, Container container);

        Task DeleteItemAsync(string id, string userId, Container container);

        Task UpdateItem(ToDoList item, Container container);

        // META CRUD OPERATIONS
        Task<DatabaseResponse> CheckDatabase(string database);

        Task<ContainerResponse> CheckContainer(string containerName, string partitionKeyPath);

        // FETCH OPERATIONS
        Container GetContainer(string containerName);

        Task<ToDoList> GetItemAsync(string id, Container container);

        Task<List<ToDoList>> GetMyItems(string userId, Container container);

        Task<IEnumerable<ToDoList>> GetItemsAsync(string queryString, Container container);

        Task<IEnumerable<Videos>> GetUploadsAsync(string queryString, Container container);

        Task ListContainersInDatabase();
    }
}