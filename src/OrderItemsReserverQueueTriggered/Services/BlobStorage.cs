using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Options;

namespace OrderItemsReserverQueueTriggered.Services;

public class BlobStorage : IStorage
{
    private readonly AzureStorageConfig storageConfig;

    public BlobStorage(IOptions<AzureStorageConfig> storageConfig)
    {
        this.storageConfig = storageConfig.Value;
    }

    public Task Initialize()
    {
        BlobServiceClient blobServiceClient = new BlobServiceClient(storageConfig.ConnectionString);
        BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(storageConfig.FileContainerName);
        return containerClient.CreateIfNotExistsAsync();
    }

    public Task Save(Stream fileStream, string name)
    {
        BlobServiceClient blobServiceClient = new BlobServiceClient(storageConfig.ConnectionString);

        // Get the container (folder) the file will be saved in
        BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(storageConfig.FileContainerName);

        // Get the Blob Client used to interact with (including create) the blob
        BlobClient blobClient = containerClient.GetBlobClient(name);

        // Upload the blob
        return blobClient.UploadAsync(fileStream);
    }
}
