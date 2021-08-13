// Default URL for triggering event grid function in the local environment.
// http://localhost:7071/runtime/webhooks/EventGrid?functionName={functionname}
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;

namespace GoussanFunction
{
    public static class PushEvent
    {
        [FunctionName("PushEvent")]
        public static void Run(
            [EventGridTrigger] EventGridEvent gridEvent,
            [CosmosDB(databaseName: "GoussanDatabase", collectionName: "videos", ConnectionStringSetting = "CosmosDbConnectionString")] DocumentClient client,
            ILogger log)
        {
            string database = "GoussanDatabase";
            string collection = "videos";

            log.LogInformation("Processing Event Trigger from AMS");
            try
            {
                log.LogInformation("HTTP Trigger function processed a request.");

                // Extract data from EVENT
                JToken gridDataToken = JObject.FromObject(gridEvent.Data);
                JToken output = gridDataToken.SelectToken("output");
                JToken assetName = output.SelectToken("assetName");
                JToken state = output.SelectToken("state");
                string ID = assetName.ToString().Split("-").First();
                log.LogInformation($"ID Extracted successfully: {ID}");

                // Fetching Document from Cosmos DB
                log.LogInformation("Trying to get document from cosmosDB");
                Uri DocumentLink = UriFactory.CreateDocumentUri(database, collection, ID);
                RequestOptions dbVidOptions = new RequestOptions()
                {
                    PartitionKey = new PartitionKey(ID)
                };
                Videos dbVid = client.ReadDocumentAsync<Videos>(DocumentLink, dbVidOptions).GetAwaiter().GetResult();

                dbVid.OutputAsset = assetName.ToString();
                dbVid.Description = "Document has been updated by Azure Functions";
                dbVid.Status = state.ToString();
                dbVid.LastModified = gridEvent.EventTime;

                log.LogInformation($"Updating Document: {dbVid.Id}");

                Uri DocumentCollectionUri = UriFactory.CreateDocumentCollectionUri(database, collection);
                Uri DatabaseLink = UriFactory.CreateDatabaseUri(database);

                // Sending Document to Cosmos DB
                log.LogInformation("Sending Document to Cosmos DB");
                var response = client.ReplaceDocumentAsync(DocumentLink, dbVid, dbVidOptions).GetAwaiter().GetResult();
                log.LogInformation($"Cosmos Response: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message);
            }
        }

        public class Videos
        {
            [JsonProperty(PropertyName = "id")]
            public string Id { get; set; }

            [JsonProperty(PropertyName = "outputAsset")]
            public string OutputAsset { get; set; }

            [JsonProperty(PropertyName = "title")]
            public string Title { get; set; }

            [JsonProperty(PropertyName = "description")]
            public string Description { get; set; }

            [JsonProperty(PropertyName = "filename")]
            public string FileName { get; set; }

            [JsonProperty(PropertyName = "bloburi")]
            public string BlobUri { get; set; }

            [JsonProperty(PropertyName = "locator")]
            public string Locator { get; set; }

            [JsonProperty(PropertyName = "streamingurl")]
            public string StreamingUrl { get; set; }

            [JsonProperty(PropertyName = "created")]
            public DateTime UploadDate { get; set; }

            [JsonProperty(PropertyName = "lastmodified")]
            public DateTimeOffset LastModified { get; set; }

            [JsonProperty(PropertyName = "filesize")]
            public long Size { get; set; }

            [JsonProperty(PropertyName = "extension")]
            public string Extension { get; set; }

            [JsonProperty(PropertyName = "status")]
            public string Status { get; set; }
        }
    }
}