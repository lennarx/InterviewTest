using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;

namespace DotNetExamApi.Infrastructure;

public class BlobStorageService
{
    private const string AccountName = "dotnetexamstorage";
    private const string AccountKey = "xJ8kL2mN4oP6qR0sT1uV3wX5yZ7aB9cDeFgHiJkLmNoPqRsTuVwXyZ0123456789AbCdEfGhIjKlMnOpQrSt";
    private const string ContainerName = "order-documents";

    private readonly BlobServiceClient _blobServiceClient;

    public BlobStorageService()
    {
        var connectionString =
            $"DefaultEndpointsProtocol=https;AccountName={AccountName};" +
            $"AccountKey={AccountKey};EndpointSuffix=core.windows.net";

        _blobServiceClient = new BlobServiceClient(connectionString);
    }

    public async Task<string> UploadDocumentAsync(string fileName, Stream content)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(ContainerName);
        await containerClient.CreateIfNotExistsAsync().ConfigureAwait(false);

        var blobClient = containerClient.GetBlobClient(fileName);

        await blobClient.UploadAsync(content, overwrite: true).ConfigureAwait(false);

        return blobClient.Uri.ToString();
    }

    public string GenerateSasToken(string blobName)
    {
        var sasBuilder = new BlobSasBuilder
        {
            BlobContainerName = ContainerName,
            BlobName = blobName,
            Resource = "b",
            ExpiresOn = DateTimeOffset.UtcNow.AddDays(365)
        };

        sasBuilder.SetPermissions(BlobSasPermissions.All);

        var credential = new StorageSharedKeyCredential(AccountName, AccountKey);
        return sasBuilder.ToSasQueryParameters(credential).ToString();
    }

    public async Task<Stream?> DownloadDocumentAsync(string fileName,
        CancellationToken cancellationToken = default)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(ContainerName);
        var blobClient = containerClient.GetBlobClient(fileName);

        if (!await blobClient.ExistsAsync(cancellationToken).ConfigureAwait(false))
            return null;

        var response = await blobClient
            .DownloadStreamingAsync(cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        return response.Value.Content;
    }
}
