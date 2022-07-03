using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Azure.Cosmos;
using System.Net;
using api.Image.Models;


public class ImageRepository : IImageRepository
{
    private CosmosClient cosmosClient;
    private Database database;
    private Container container;
    private string databaseId = string.Empty;
    private string containerId = string.Empty;

    private IConfiguration _iconfig;
    public ImageRepository(IConfiguration iconfig)
    {
        _iconfig = iconfig;
        string connectionString = iconfig["CosmosDbConnectionString"];
        databaseId = "ToDoList";
        containerId = "Items";
        cosmosClient = new CosmosClient(connectionString, new CosmosClientOptions()
        {
            ConnectionMode = ConnectionMode.Gateway
        });
        CreateDatabaseAsync().Wait();
        CreateContainerAsync().Wait();

    }

    private async Task CreateDatabaseAsync()
    {
        this.database = await this.cosmosClient.CreateDatabaseIfNotExistsAsync(databaseId);
    }
    private async Task CreateContainerAsync()
    {
        this.container = await this.database.CreateContainerIfNotExistsAsync(containerId, "/partitionKey");
    }


    public async Task CreateImage(Image image)
    {
        try
        {
            ItemResponse<Image> itemResponse = await container.ReadItemAsync<Image>(image.md5, new PartitionKey(image.partitionKey));

        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            await container.CreateItemAsync(image, new PartitionKey(image.partitionKey));

        }

    }

    public async Task Delete(Image image)
    {
        var partitionKeyValue = image.partitionKey;
        var id = image.id;

        await this.container.DeleteItemAsync<Image>(id, new PartitionKey(partitionKeyValue));

    }

//aggiungere il parametro pk, non fare mai select *!
    public async Task<List<Image>> GetImages(string partitionKey)
    {
        //var qry = "SELECT * FROM c";
        var qry = string.Format("SELECT * FROM c WHERE c.md5 = '{0}'", partitionKey);
        QueryDefinition queryDefinition = new QueryDefinition(qry);
        FeedIterator<Image> queryIterator = container.GetItemQueryIterator<Image>(queryDefinition);
        List<Image> result = new List<Image>();
        while (queryIterator.HasMoreResults)
        {
            FeedResponse<Image> resultSet = await queryIterator.ReadNextAsync();
            foreach (Image imageRes in resultSet)
            {
                result.Add(imageRes);
            }
        }
        return result;



    }
    public async Task<Image> GetImage(string md5)
    {
        try
        {
            var qry = string.Format("SELECT * FROM c WHERE c.md5 = '{0}'", md5);
            QueryDefinition queryDefinition = new QueryDefinition(qry);
            FeedIterator<Image> queryIterator = container.GetItemQueryIterator<Image>(queryDefinition);
            Image result = new Image();
            while (queryIterator.HasMoreResults)
            {
                FeedResponse<Image> resultSet = await queryIterator.ReadNextAsync();
                foreach (Image imageRes in resultSet)
                {
                    result.id = imageRes.id;
                    result.partitionKey = imageRes.partitionKey;
                    result.Name = imageRes.Name;
                    result.Size = imageRes.Size;
                    result.Path = imageRes.Path;
                    result.TimeStamp = imageRes.TimeStamp;
                    result.md5 = imageRes.md5;
                    result.operation = imageRes.operation;
                }
            }
            return result;

        }
        catch (CosmosException ex)
        {
            throw new System.Exception(string.Format("Error occurs :: {0}", ex.Message));
        }
    }



    public async Task UpdateImage(Image image, string md5, string partitionKey)
    {
       ItemResponse<Image> response = await this.container.ReadItemAsync<Image>(md5, new PartitionKey(partitionKey));

       var result = response.Resource;
       result.id = image.id;
       result.partitionKey = image.partitionKey;
       result.Name = image.Name;
       result.Size = image.Size;
       result.Path = image.Path;
       result.TimeStamp = image.TimeStamp;
       result.md5 = image.md5;
       result.operation = image.operation;

       await this.container.ReplaceItemAsync<Image>(result,result.md5, new PartitionKey(image.partitionKey));

    }
}