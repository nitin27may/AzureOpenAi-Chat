using GenAI.Api.Models;

namespace GenAI.Api.Services;

public interface IBlobService
{
    Task<string> UploadAsync(Stream fileStream, string containerName, string fileName, string contentType);

    Task<List<BlobFile>> GetFiles(string containerName, int? segmentSize);

}