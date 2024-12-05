using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage.Sas;
using GenAI.Api.Models;
using System.Net;
using System.Text.RegularExpressions;

namespace GenAI.Api.Services;

public class BlobService : IBlobService
{
    private readonly string _storageConnectionString;
    private readonly ILogger<BlobService> _logger;
    private readonly BlobServiceClient _blobServiceClient;

    public BlobService(IConfiguration configuration, ILogger<BlobService> logger)
    {
        _storageConnectionString = configuration.GetConnectionString("AzureStorage");
        _logger = logger;
        _blobServiceClient = new BlobServiceClient(_storageConnectionString);
    }

    public async Task<List<BlobFile>> GetFiles(string containerName, int? segmentSize = null)
    {
        if (!IsContainerNameValid(containerName))
        {
            _logger.LogError($"Invalid Container Name: {containerName}.");
            throw new HttpStatusException(HttpStatusCode.BadRequest, $"Invalid Container Name: {containerName}");
        }

        List<BlobFile> files = new List<BlobFile>();
        BlobContainerClient blobContainerClient = _blobServiceClient.GetBlobContainerClient(containerName);

        if (!await blobContainerClient.ExistsAsync())
        {
            _logger.LogError($"Container {containerName} does not exist.");
            throw new HttpStatusException(HttpStatusCode.NotFound, $"Container {containerName} does not exist.");
        }

        var resultSegment = blobContainerClient.GetBlobsAsync().AsPages(default, segmentSize);

        await foreach (Azure.Page<BlobItem> blobPage in resultSegment)
        {
            foreach (BlobItem blobItem in blobPage.Values)
            {
                BlobClient blob = blobContainerClient.GetBlobClient(blobItem.Name);
                files.Add(new BlobFile { Name = blobItem.Name, Uri = GetSASToken(blob), CreatedOn = blobItem.Properties.CreatedOn });
            }
        }

        return files;
    }

    public async Task<string> UploadAsync(Stream fileStream, string containerName, string fileName, string contentType)
    {
        if (!IsContainerNameValid(containerName))
        {
            _logger.LogError($"Invalid Container Name: {containerName}.");
            throw new HttpStatusException(HttpStatusCode.BadRequest, $"Invalid Container Name: {containerName}");
        }

        BlobContainerClient containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        await containerClient.CreateIfNotExistsAsync();

        try
        {
            //var policy = new PublicAccessType();
            await containerClient.SetAccessPolicyAsync(PublicAccessType.None);
            var blob = containerClient.GetBlobClient(fileName);
            await blob.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots);
            await blob.UploadAsync(fileStream, new BlobUploadOptions { HttpHeaders = new BlobHttpHeaders { ContentType = contentType } });
            var url = GetSASToken(blob);
            return url.ToString();
        }
        catch (Exception exception)
        {
            _logger.LogError($"Error While uploading file: {fileName} in container: {containerName}", exception.Message);
            throw new HttpStatusException(HttpStatusCode.BadRequest, exception.Message);
        }
    }

    private bool IsContainerNameValid(string name)
    {
        return Regex.IsMatch(name, "^[a-z0-9](?!.*--)[a-z0-9-]{1,61}[a-z0-9]$", RegexOptions.Singleline | RegexOptions.CultureInvariant);
    }

    private Uri GetSASToken(BlobClient blobClient)
    {
        BlobSasBuilder sasBuilder = new BlobSasBuilder()
        {
            BlobContainerName = blobClient.GetParentBlobContainerClient().Name,
            BlobName = blobClient.Name,
            Resource = "b",
            ExpiresOn = DateTimeOffset.UtcNow.AddHours(1),
        };
        sasBuilder.SetPermissions(BlobSasPermissions.Read | BlobSasPermissions.Write);

        Uri sasUri = blobClient.GenerateSasUri(sasBuilder);
        return sasUri;
    }
}