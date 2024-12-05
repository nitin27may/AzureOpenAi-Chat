using Azure;
using Azure.AI.FormRecognizer.DocumentAnalysis;
using System.Data;

namespace GenAI.Api.Services;

public class DocumentIntelligenceService : IDocumentIntelligenceService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger _logger;

    public DocumentIntelligenceService(IConfiguration configuration, ILogger<DocumentIntelligenceService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<Dictionary<string, object>> ExtractDetails(string fileURL, string modelId)
    {
        string endpoint = _configuration.GetValue<string>("DocumentIntelligence:Endpoint");
        string apiKey = _configuration.GetValue<string>("DocumentIntelligence:ApiKey");
        AzureKeyCredential credential = new AzureKeyCredential(apiKey);
        DocumentAnalysisClient client = new DocumentAnalysisClient(new Uri(endpoint), credential);

        // string modelId = _configuration.GetValue<string>("DocumentIntelligence:ModelId");
        Uri fileUri = new Uri(fileURL);

        AnalyzeDocumentOperation operation = await client.AnalyzeDocumentFromUriAsync(
            WaitUntil.Completed,
            modelId,
            fileUri
            );
        AnalyzeResult result = operation.Value;

        Console.WriteLine($"Document was analyzed with model with ID: {result.ModelId}");

        Dictionary<string, dynamic> myDict = new Dictionary<string, dynamic>();
        foreach (AnalyzedDocument document in result.Documents)
        {
            Console.WriteLine($"Document of type: {document.DocumentType}");

            foreach (var fieldKvp in document.Fields)
            {
                string fieldName = fieldKvp.Key;
                var field = fieldKvp.Value;
                myDict.Add(fieldName, field.Content);
            }
        }
        myDict["extractedDetails"] = GetDataTable(result);

        return myDict;
    }

    public async Task<string> ReadFile(string fileURL)
    {
        string endpoint = _configuration.GetValue<string>("DocumentIntelligence:Endpoint");
        string apiKey = _configuration.GetValue<string>("DocumentIntelligence:ApiKey");
        AzureKeyCredential credential = new AzureKeyCredential(apiKey);
        DocumentAnalysisClient client = new DocumentAnalysisClient(new Uri(endpoint), credential);

        Uri fileUri = new Uri(fileURL);

        AnalyzeDocumentOperation operation = await client.AnalyzeDocumentFromUriAsync(WaitUntil.Completed, "prebuilt-read", fileUri);

        AnalyzeResult result = operation.Value;
        return result.Content;
    }

    private DataTable GetDataTable(AnalyzeResult result)
    {
        DataTable dataTable = new DataTable("ScheduleOfCoverage");
        foreach (AnalyzedDocument document in result.Documents)
        {
            var fields = new Dictionary<string, object>();
            foreach (KeyValuePair<string, DocumentField> fieldKvp in document.Fields)
            {
                string fieldName = fieldKvp.Key;
                if (fieldName == "ScheduleOfCoverage")
                {
                    DocumentTable table = (DocumentTable)result.Tables[10];
                    var tableConsoleDetails = new List<Dictionary<string, object>>();

                    foreach (DocumentTableCell cell in table.Cells)
                    {
                        var cellDetails = new Dictionary<string, object>
                        {
                            { "RowIndex", cell.RowIndex },
                            { "ColumnIndex", cell.ColumnIndex },
                            { "Content", cell.Content }
                        };
                        tableConsoleDetails.Add(cellDetails);
                    }
                    fields["TableDetailsConsole"] = tableConsoleDetails;

                    var res = tableConsoleDetails.GroupBy(x => x["RowIndex"]).Select(x => x.ToList()).ToList();
                    for (int i = 0; i < res.Count; i++)
                    {
                        if (i == 0)
                        {
                            var headerArray = res[0].Select(x => x["Content"]).ToArray();
                            dataTable.Columns.Add(headerArray[0].ToString(), typeof(string));
                            dataTable.Columns.Add(headerArray[1].ToString(), typeof(string));
                            dataTable.Columns.Add(headerArray[2].ToString(), typeof(string));
                        }
                        else
                        {
                            var rowArray = res[i].Select(x => x["Content"]).ToArray();
                            dataTable.Rows.Add(rowArray[0].ToString(), rowArray[1].ToString(), rowArray[2].ToString());
                        }
                    }
                }
            }
        }
        return dataTable;
    }
}