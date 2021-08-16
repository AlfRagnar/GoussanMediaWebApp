# GoussanMediaWebApp
This is a simple Video Sharing Platform using ASP.NET Blazor-server with Azure services backend. Most of the code is C# and is split to separate concerns more easily.

#### Project Overview
* GoussanMedia - The Server/Client app, this is where the pages are defined and startup protocols are set
* GoussanMedia.DataAccess - This is the data layer that handles the communication with services like Azure Media Services, Cosmos DB and Azure Storage
* GoussanMedia.Domain - This is where the models are defined and stored. These models are used throughout the project.
* GoussanFunction - This is the Azure Functions that is receiving events from Azure Media upon scheduling and finishing a job ( Encoding ) and then updating the data in Cosmos DB





#### Current Services in Use:
* Cosmos DB
* Azure Keyvault
* Azure Application Insights
* Azure SignalR Service
* Azure Media Services
* Azure Functions

#### Packages in Use:
* Azure Media Player
* MudBlazor
 

### How to Get Started
* Setup Microsoft Azure Subscription
* Setup Cosmos DB account and Subscription
* Setup Keyvault ( This will be used to store your secrets, connection  strings and keys )
* Setup Azure Storage account ( This will be used to store your diagnostics and the files users upload )
* Setup Azure SignalR Service ( This is used by Blazor-server to handle client communication. Dev/Free has a limit of 20k Message per day / Unit while standard tier is 1000k message per day / unit )
* Setup Azure Media Services ( This will be used to Encode the videos uploaded  by the users, handle the creation of streaming URL, create the asset in Azure Blob Storage etc )
* Setup Azure Functions ( This will be needed to handle events from Azure Media Service to update the data in Cosmos DB )
* Clone the github Code and open the solution in Visual Studio
* Setup the Connected Services so that they connect to your Azure Accounts and use the services required on the page
* Restore the Nuget Packages
* And you should be good to go!


### Limitations
The code here is free to use and is delivered as is. Most of the limitations on this app is from the services in use, mainly due to the SignalR service and message limit. File upload among other stuff will take a huge chunk due to the way it is handled. Would love to just let the client upload the file directly to storage, but that would be a little risky.
