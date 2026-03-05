using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using Azure.Search.Documents.Models;

/// <summary>
/// CLI tool to seed emission factor data into Azure AI Search.
/// Usage: dotnet run --project tools/GreenLens.Seed
/// </summary>

var endpoint = Environment.GetEnvironmentVariable("AZURE_SEARCH_ENDPOINT")
    ?? throw new InvalidOperationException("AZURE_SEARCH_ENDPOINT not set. Set it in .env or environment.");
var apiKey = Environment.GetEnvironmentVariable("AZURE_SEARCH_API_KEY")
    ?? throw new InvalidOperationException("AZURE_SEARCH_API_KEY not set.");
var indexName = Environment.GetEnvironmentVariable("AZURE_SEARCH_INDEX_NAME") ?? "emission-factors";

Console.WriteLine($"Connecting to Azure AI Search: {endpoint}");
Console.WriteLine($"Index: {indexName}");

var credential = new AzureKeyCredential(apiKey);
var indexClient = new SearchIndexClient(new Uri(endpoint), credential);

// Step 1: Create or update the index
Console.WriteLine("Creating/updating search index...");
var indexDefinition = new SearchIndex(indexName)
{
    Fields = new List<SearchField>
    {
        new SimpleField("id", SearchFieldDataType.String) { IsKey = true, IsFilterable = true },
        new SearchableField("resourceType") { IsFilterable = true, IsSortable = true },
        new SearchableField("provider") { IsFilterable = true },
        new SearchableField("region") { IsFilterable = true },
        new SimpleField("co2ePerUnit", SearchFieldDataType.Double) { IsFilterable = true, IsSortable = true },
        new SearchableField("unit") { IsFilterable = true },
        new SearchableField("source") { IsFilterable = true },
        new SimpleField("effectiveDate", SearchFieldDataType.DateTimeOffset) { IsFilterable = true, IsSortable = true },
    }
};

await indexClient.CreateOrUpdateIndexAsync(indexDefinition);
Console.WriteLine("Index created/updated successfully.");

// Step 2: Load CSV data
var csvPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "data", "emission-factors", "azure-emission-factors.csv");

if (!File.Exists(csvPath))
{
    // Try relative to working directory
    csvPath = Path.Combine(Directory.GetCurrentDirectory(), "data", "emission-factors", "azure-emission-factors.csv");
}

if (!File.Exists(csvPath))
{
    Console.WriteLine($"ERROR: CSV file not found. Tried: {csvPath}");
    Console.WriteLine("Run from the project root directory.");
    return 1;
}

Console.WriteLine($"Loading data from: {csvPath}");

var lines = File.ReadAllLines(csvPath).Skip(1); // Skip header
var documents = new List<SearchDocument>();
var count = 0;

foreach (var line in lines)
{
    if (string.IsNullOrWhiteSpace(line)) continue;

    var parts = line.Split(',');
    if (parts.Length < 7) continue;

    var id = $"{parts[0].Trim()}-{parts[2].Trim()}".ToLowerInvariant().Replace(" ", "-");

    var doc = new SearchDocument
    {
        ["id"] = id,
        ["resourceType"] = parts[0].Trim(),
        ["provider"] = parts[1].Trim(),
        ["region"] = parts[2].Trim(),
        ["co2ePerUnit"] = double.Parse(parts[3].Trim()),
        ["unit"] = parts[4].Trim(),
        ["source"] = parts[5].Trim(),
        ["effectiveDate"] = DateTimeOffset.Parse(parts[6].Trim())
    };

    documents.Add(doc);
    count++;
}

Console.WriteLine($"Parsed {count} emission factors from CSV.");

// Step 3: Upload to Azure AI Search
var searchClient = new SearchClient(new Uri(endpoint), indexName, credential);

Console.WriteLine("Uploading documents to Azure AI Search...");
var batch = IndexDocumentsBatch.MergeOrUpload(documents);
var result = await searchClient.IndexDocumentsAsync(batch);

var succeeded = result.Value.Results.Count(r => r.Succeeded);
var failed = result.Value.Results.Count(r => !r.Succeeded);

Console.WriteLine($"Upload complete: {succeeded} succeeded, {failed} failed.");

if (failed > 0)
{
    foreach (var failedResult in result.Value.Results.Where(r => !r.Succeeded))
    {
        Console.WriteLine($"  FAILED: {failedResult.Key} - {failedResult.ErrorMessage}");
    }
}

Console.WriteLine("Seed complete!");
return 0;
