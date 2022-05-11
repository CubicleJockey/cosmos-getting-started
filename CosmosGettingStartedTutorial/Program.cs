using System;
using System.Threading.Tasks;
using System.Configuration;
using System.Collections.Generic;
using System.Net;
using CosmosGettingStartedTutorial.Entities;
using Microsoft.Azure.Cosmos;

using static System.Console;

namespace CosmosGettingStartedTutorial
{
    public class Program
    {
        // The Azure Cosmos DB endpoint for running this sample.
        private static readonly string EndpointUri = ConfigurationManager.AppSettings["EndPointUri"];

        // The primary key for the Azure Cosmos account.
        private static readonly string PrimaryKey = ConfigurationManager.AppSettings["PrimaryKey"];

        // The Cosmos client instance
        private CosmosClient cosmosClient;

        // The database we will create
        private Database database;

        // The container we will create.
        private Container container;

        // The name of the database and container we will create
        private const string databaseId = "db";
        private const string containerId = "items";

        // <Main>
        public static async Task Main(string[] args)
        {
            try
            {
                WriteLine($"Beginning operations...{Environment.NewLine}");
                var p = new Program();
                await p.GetStartedDemoAsync();

            }
            catch (CosmosException de)
            {
                var baseException = de.GetBaseException();
                WriteLine($"{de.StatusCode} error occurred: {baseException.Message}");
            }
            catch (Exception e)
            {
                WriteLine($"Error: {e}");
            }
            finally
            {
                WriteLine("End of demo, press any key to exit.");
                ReadKey();
            }
        }
        // </Main>

        // <GetStartedDemoAsync>
        /// <summary>
        /// Entry point to call methods that operate on Azure Cosmos DB resources in this sample
        /// </summary>
        public async Task GetStartedDemoAsync()
        {
            // Create a new instance of the Cosmos Client
            cosmosClient = new CosmosClient(EndpointUri, PrimaryKey, new CosmosClientOptions
            {
                ApplicationName = "CosmosDBDotnetQuickstart"
            });
            await CreateDatabaseAsync();
            await CreateContainerAsync();
            await ScaleContainerAsync();
            await AddItemsToContainerAsync();
            await QueryItemsAsync();
            await ReplaceFamilyItemAsync();
            await DeleteFamilyItemAsync();


            WriteLine("Press any key to cleanup database!");
            ReadKey();
            await DeleteDatabaseAndCleanupAsync();
        }
        // </GetStartedDemoAsync>

        // <CreateDatabaseAsync>
        /// <summary>
        /// Create the database if it does not exist
        /// </summary>
        private async Task CreateDatabaseAsync()
        {
            // Create a new database
            database = await cosmosClient.CreateDatabaseIfNotExistsAsync(databaseId);
            WriteLine($"Created Database: {database.Id}{Environment.NewLine}");
        }
        // </CreateDatabaseAsync>

        // <CreateContainerAsync>
        /// <summary>
        /// Create the container if it does not exist. 
        /// Specifiy "/LastName" as the partition key since we're storing family information, to ensure good distribution of requests and storage.
        /// </summary>
        /// <returns></returns>
        private async Task CreateContainerAsync()
        {
            // Create a new container
            container = await database.CreateContainerIfNotExistsAsync(containerId, "/LastName", 400);
            WriteLine($"Created Container: {container.Id}{Environment.NewLine}");
        }
        // </CreateContainerAsync>

        // <ScaleContainerAsync>
        /// <summary>
        /// Scale the throughput provisioned on an existing Container.
        /// You can scale the throughput (RU/s) of your container up and down to meet the needs of the workload. Learn more: https://aka.ms/cosmos-request-units
        /// </summary>
        /// <returns></returns>
        private async Task ScaleContainerAsync()
        {
            // Read the current throughput
            var throughput = await container.ReadThroughputAsync();
            if (throughput.HasValue)
            {
                WriteLine($"Current provisioned throughput : {throughput.Value}{Environment.NewLine}");
                var newThroughput = throughput.Value + 100;
                // Update throughput
                await container.ReplaceThroughputAsync(newThroughput);
                WriteLine($"New provisioned throughput : {newThroughput}{Environment.NewLine}");
            }
            
        }
        // </ScaleContainerAsync>

        // <AddItemsToContainerAsync>
        /// <summary>
        /// Add Family items to the container
        /// </summary>
        private async Task AddItemsToContainerAsync()
        {
            // Create a family object for the Andersen family
            var andersenFamily = new Family
            {
                Id = "Andersen.1",
                LastName = "Andersen",
                Parents = new Parent[]
                {
                    new() { FirstName = "Thomas" },
                    new() { FirstName = "Mary Kay" }
                },
                Children = new Child[]
                {
                    new()
                    {
                        FirstName = "Henriette Thaulow",
                        Gender = "female",
                        Grade = 5,
                        Pets = new Pet[]
                        {
                            new() { GivenName = "Fluffy" }
                        }
                    }
                },
                Address = new Address { State = "WA", County = "King", City = "Seattle" },
                IsRegistered = false
            };

            try
            {
                // Read the item to see if it exists.  
                var andersenFamilyResponse = await container.ReadItemAsync<Family>(andersenFamily.Id, new PartitionKey(andersenFamily.LastName));
                WriteLine($"Item in database with id: {andersenFamilyResponse.Resource.Id} already exists{Environment.NewLine}");
            }
            catch(CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                // Create an item in the container representing the Andersen family. Note we provide the value of the partition key for this item, which is "Andersen"
                var andersenFamilyResponse = await container.CreateItemAsync(andersenFamily, new PartitionKey(andersenFamily.LastName));

                // Note that after creating the item, we can access the body of the item with the Resource property off the ItemResponse. We can also access the RequestCharge property to see the amount of RUs consumed on this request.
                WriteLine($"Created item in database with id: {andersenFamilyResponse.Resource.Id} Operation consumed {andersenFamilyResponse.RequestCharge} RUs.{Environment.NewLine}");
            }

            // Create a family object for the Wakefield family
            var wakeFieldFamily = new Family
            {
                Id = "Wakefield.7",
                LastName = "Wakefield",
                Parents = new Parent[]
                {
                    new() { FamilyName = "Wakefield", FirstName = "Robin" },
                    new() { FamilyName = "Miller", FirstName = "Ben" }
                },
                Children = new Child[]
                {
                    new()
                    {
                        FamilyName = "Merriam",
                        FirstName = "Jesse",
                        Gender = "female",
                        Grade = 8,
                        Pets = new Pet[]
                        {
                            new() { GivenName = "Goofy" },
                            new() { GivenName = "Shadow" }
                        }
                    },
                    new()
                    {
                        FamilyName = "Miller",
                        FirstName = "Lisa",
                        Gender = "female",
                        Grade = 1
                    }
                },
                Address = new Address { State = "NY", County = "Manhattan", City = "NY" },
                IsRegistered = true
            };

            try
            {
                // Read the item to see if it exists
                var wakeFieldFamilyResponse = await container.ReadItemAsync<Family>(wakeFieldFamily.Id, new PartitionKey(wakeFieldFamily.LastName));
                WriteLine($"Item in database with id: {wakeFieldFamilyResponse.Resource.Id} already exists{Environment.NewLine}");
            }
            catch(CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                // Create an item in the container representing the Wakefield family. Note we provide the value of the partition key for this item, which is "Wakefield"
                var wakeFieldFamilyResponse = await container.CreateItemAsync<Family>(wakeFieldFamily, new PartitionKey(wakeFieldFamily.LastName));

                // Note that after creating the item, we can access the body of the item with the Resource property off the ItemResponse. We can also access the RequestCharge property to see the amount of RUs consumed on this request.
                WriteLine($"Created item in database with id: {wakeFieldFamilyResponse.Resource.Id} Operation consumed {wakeFieldFamilyResponse.RequestCharge} RUs.{Environment.NewLine}");
            }
        }
        // </AddItemsToContainerAsync>

        // <QueryItemsAsync>
        /// <summary>
        /// Run a query (using Azure Cosmos DB SQL syntax) against the container
        /// Including the partition key value of lastName in the WHERE filter results in a more efficient query
        /// </summary>
        private async Task QueryItemsAsync()
        {
            var sqlQueryText = "SELECT * FROM c WHERE c.LastName = 'Andersen'";

            WriteLine($"Running query: [{sqlQueryText}]{Environment.NewLine}");

            var queryDefinition = new QueryDefinition(sqlQueryText);
            var queryResultSetIterator = container.GetItemQueryIterator<Family>(queryDefinition);

            IList<Family> families = new List<Family>();

            while (queryResultSetIterator.HasMoreResults)
            {
                var currentResultSet = await queryResultSetIterator.ReadNextAsync();
                foreach (var family in currentResultSet)
                {
                    families.Add(family);
                    WriteLine($"\tRead {family}{Environment.NewLine}");
                }
            }
        }
        // </QueryItemsAsync>

        // <ReplaceFamilyItemAsync>
        /// <summary>
        /// Replace an item in the container
        /// </summary>
        private async Task ReplaceFamilyItemAsync()
        {
            var wakeFieldFamilyResponse = await container.ReadItemAsync<Family>("Wakefield.7", new PartitionKey("Wakefield"));
            var itemBody = wakeFieldFamilyResponse.Resource;
            
            // update registration status from false to true
            itemBody.IsRegistered = true;
            // update grade of child
            itemBody.Children[0].Grade = 6;

            // replace the item with the updated content
            wakeFieldFamilyResponse = await container.ReplaceItemAsync<Family>(itemBody, itemBody.Id, new PartitionKey(itemBody.LastName));
            WriteLine($"Updated Family [{itemBody.LastName},{itemBody.Id}].{Environment.NewLine} \tBody is now: {wakeFieldFamilyResponse.Resource}{Environment.NewLine}");
        }
        // </ReplaceFamilyItemAsync>

        // <DeleteFamilyItemAsync>
        /// <summary>
        /// Delete an item in the container
        /// </summary>
        private async Task DeleteFamilyItemAsync()
        {
            const string partitionKeyValue = "Wakefield";
            const string familyId = "Wakefield.7";

            // Delete an item. Note we must provide the partition key value and id of the item to delete
            _ = await container.DeleteItemAsync<Family>(familyId,new PartitionKey(partitionKeyValue));
            WriteLine($"Deleted Family [{partitionKeyValue},{familyId}]{Environment.NewLine}");
        }
        // </DeleteFamilyItemAsync>

        // <DeleteDatabaseAndCleanupAsync>
        /// <summary>
        /// Delete the database and dispose of the Cosmos Client instance
        /// </summary>
        private async Task DeleteDatabaseAndCleanupAsync()
        {
            _ = await database.DeleteAsync();
            // Also valid: await cosmosClient.Databases["FamilyDatabase"].DeleteAsync();

            WriteLine($"Deleted Database: {databaseId}{Environment.NewLine}");

            //Dispose of CosmosClient
            cosmosClient.Dispose();
        }
        // </DeleteDatabaseAndCleanupAsync>
    }
}
