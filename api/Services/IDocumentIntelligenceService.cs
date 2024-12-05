using Azure.AI.FormRecognizer.DocumentAnalysis;

namespace GenAI.Api.Services;

public interface IDocumentIntelligenceService
{
    Task<Dictionary<string, object>> ExtractDetails(string fileURL, string modelId);

    Task<string> ReadFile(string fileURL);
}