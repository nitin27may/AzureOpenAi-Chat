using DocumentFormat.OpenXml.Packaging;
using GenAI.Api.Extensions;
using GenAI.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace GenAI.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FormController : ControllerBase
{
    private readonly IBlobService _blobService;
    private readonly IDocumentIntelligenceService _documentIntelligenceService;
    private readonly IOpenApiService _openApiService;
    private readonly ILogger _logger;
    private readonly IConfiguration _configuration;

    public FormController(IBlobService blobService,
        ILogger<FormController> logger,
        IDocumentIntelligenceService documentIntelligenceService,
         IOpenApiService openApiService,
        IConfiguration configuration)
    {
        _blobService = blobService;
        _logger = logger;
        _documentIntelligenceService = documentIntelligenceService;
        _openApiService = openApiService;
        _configuration = configuration;
    }


    [HttpPost("read"), DisableRequestSizeLimit]
    public async Task<IActionResult> ReadDocumentContent([FromForm] FileModel fileModel)
    {
        try
        {
            string fileURL = await _blobService.UploadAsync(fileModel.File.OpenReadStream(), "documents", fileModel.File.FileName.AppendTimeStamp(), fileModel.File.ContentType);
            var rowKey = Guid.NewGuid().ToString();
            var result = await _documentIntelligenceService.ReadFile(fileURL);

            return Ok(result);
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }


    [HttpPost("query")]
    public async Task<IActionResult> GetResponse(QueryRequest queryRequest)
    {
        try
        {
            var result = await _openApiService.GetChatCompletion(queryRequest.InputString);

            return Ok(new { response = result });
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    [HttpPost("compare")]
    public async Task CompareFiles([FromForm] List<IFormFile> files, [FromForm] string customPrompt = "Compare the texts and identify the differences.")
    {
        string prompt = string.Empty;
        if (files != null && files.Any())
        {
            // Iterate through the uploaded files and extract their text
            for (int i = 0; i < files.Count; i++)
            {
                var fileText = await ExtractTextFromFile(files[i]);
                prompt += $"\n\nDocument {i + 1}:\n{fileText}";
            }
        }

        // Combine the custom prompt with extracted file texts
        var finalPrompt = $"{customPrompt}{prompt}";

        Response.ContentType = "text/plain";
        Response.Headers.Add("Cache-Control", "no-cache");
        Response.Headers.Add("Transfer-Encoding", "chunked");

        // Send the prompt to OpenAI for streaming completion
        await _openApiService.GetChatStreamCompletion(finalPrompt, Response.Body, _logger);
    }

    [HttpGet("querystream")]
    [Produces("text/plain")]
    public async Task StreamChatResponse([FromQuery] string prompt)
    {
        Response.ContentType = "text/plain";
        Response.Headers.Add("Cache-Control", "no-cache");
        Response.Headers.Add("Transfer-Encoding", "chunked");

        try
        {
            await _openApiService.GetChatStreamCompletion(prompt, Response.Body, _logger);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while streaming response.");
            Response.StatusCode = StatusCodes.Status500InternalServerError;
            await Response.Body.WriteAsync(System.Text.Encoding.UTF8.GetBytes("Error occurred while streaming."));
        }
    }

    private async Task<string> ExtractTextFromFile(IFormFile file)
    {
        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);
        if (file.FileName.EndsWith(".pdf"))
        {
            string fileURL = await _blobService.UploadAsync(file.OpenReadStream(), "documents", file.FileName.AppendTimeStamp(), file.ContentType);
            var rowKey = Guid.NewGuid().ToString();
            return await _documentIntelligenceService.ReadFile(fileURL);

        }
        else if (file.FileName.EndsWith(".docx"))
        {
            using var wordDoc = WordprocessingDocument.Open(memoryStream, false);
            return wordDoc.MainDocumentPart.Document.Body.InnerText;
        }
        return string.Empty;
    }
}

public class FileModel
{
    public string? ModelId { get; set; }
    public IFormFile File { get; set; }
}

public class QueryRequest
{
    public string InputString { get; set; }
}