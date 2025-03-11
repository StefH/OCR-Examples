using System.ClientModel;
using System.ClientModel.Primitives;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OlmOcrExample;
using OpenAI;

await using var serviceProvider = RegisterServices(args);

var analyzer = serviceProvider.GetRequiredService<OlmOcrImageAnalyzer>();

var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
var folder = Path.Combine(userProfile, "OneDrive", "Training", "Generative AI Foundations", "Images");

var sourceImagePaths = Directory.GetFiles(folder, "*.png");

await using var writer = new StreamWriter(Path.Combine(folder, "output.md"));

foreach (var sourceImagePath in sourceImagePaths)
{
    Console.WriteLine(sourceImagePath);

    var text = await analyzer.AnalyzeImageAsync(sourceImagePath);
    writer.WriteLine(text);
    writer.WriteLine();
    writer.WriteLine("---");
    await writer.FlushAsync();
}

return;

static ServiceProvider RegisterServices(string[] args)
{
    var services = new ServiceCollection();

    services.AddLogging(builder => builder.AddConsole());
    services.AddSingleton(
        new OpenAIClient(new ApiKeyCredential("not needed for LM Studio"),
        new OpenAIClientOptions
        {
            Endpoint = new Uri("http://127.0.0.1:1234/v1"),
            NetworkTimeout = TimeSpan.FromMinutes(5),
            RetryPolicy = new ClientRetryPolicy(5)
        }));

    services.AddSingleton<OlmOcrImageAnalyzer>();

    return services.BuildServiceProvider();
}