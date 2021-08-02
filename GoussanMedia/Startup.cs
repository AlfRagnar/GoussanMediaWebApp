using Azure.Core.Extensions;
using Azure.Storage.Blobs;
using Azure.Storage.Queues;
using GoussanMedia.DataAccess;
using GoussanMedia.DataAccess.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;

namespace GoussanMedia
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();
            services.AddServerSideBlazor();
            services.AddHttpClient();

            // Add  Cosmos Db Service
            services.AddSingleton<ICosmosDbService>(InitializeCosmosClientInstanceAsync().GetAwaiter().GetResult());

            services.AddAzureClients(builder =>
            {
                builder.AddBlobServiceClient(Configuration["GoussanStorage"], preferMsi: true);
                //builder.AddBlobServiceClient(Configuration["GoussanStorage:blob"], preferMsi: true);
                builder.AddQueueServiceClient(Configuration["GoussanStorage:queue"], preferMsi: true);
            });
            services.AddApplicationInsightsTelemetry(Configuration["APPINSIGHTS_CONNECTIONSTRING"]);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });
        }

        private async Task<CosmosDbService> InitializeCosmosClientInstanceAsync()
        {
            // Define Azure Cosmos Db Client options like preferred operation region and Application Name
            CosmosClientOptions options = new()
            {
                ApplicationName = "GoussanMedia",
                ApplicationRegion = Regions.WestEurope
            };
            // Create the new Cosmos Db Client
            CosmosClient cosmosClient = new(Configuration["GoussanCosmos"], options);
            // Get the predefined Database name from appsettings.json
            string databaseName = Configuration["CosmosDb:DatabaseName"];
            // Initialize the client
            CosmosDbService cosmosDbService = new(cosmosClient, databaseName);
            // Check if database exists
            await cosmosDbService.CheckDatabase(databaseName);
            // Create necessary containers to store META data in
            IEnumerable<IConfiguration> containerList = Configuration.GetSection("CosmosDb").GetSection("Containers").GetChildren();
            foreach (IConfiguration item in containerList)
            {
                string containerName = item.GetSection("containerName").Value;
                string paritionKeyPath = item.GetSection("paritionKeyPath").Value;
                ContainerResponse containerResponse = await cosmosDbService.CheckContainer(containerName, paritionKeyPath);
                if (containerResponse.StatusCode != HttpStatusCode.OK)
                {
                    Trace.WriteLine(containerResponse);
                }
            }
            return cosmosDbService;
        }
    }

    internal static class StartupExtensions
    {
        public static IAzureClientBuilder<BlobServiceClient, BlobClientOptions> AddBlobServiceClient(this AzureClientFactoryBuilder builder, string serviceUriOrConnectionString, bool preferMsi)
        {
            if (preferMsi && Uri.TryCreate(serviceUriOrConnectionString, UriKind.Absolute, out Uri serviceUri))
            {
                return builder.AddBlobServiceClient(serviceUri);
            }
            else
            {
                return builder.AddBlobServiceClient(serviceUriOrConnectionString);
            }
        }

        public static IAzureClientBuilder<QueueServiceClient, QueueClientOptions> AddQueueServiceClient(this AzureClientFactoryBuilder builder, string serviceUriOrConnectionString, bool preferMsi)
        {
            if (preferMsi && Uri.TryCreate(serviceUriOrConnectionString, UriKind.Absolute, out Uri serviceUri))
            {
                return builder.AddQueueServiceClient(serviceUri);
            }
            else
            {
                return builder.AddQueueServiceClient(serviceUriOrConnectionString);
            }
        }
    }
}