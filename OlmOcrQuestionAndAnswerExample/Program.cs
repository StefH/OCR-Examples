using System.ClientModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OlmOcrExample;
using OpenAI;

await using var serviceProvider = RegisterServices(args);

var analyzer = serviceProvider.GetRequiredService<OlmOcrQuestionAndAnswerService>();

var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
var folder = Path.Combine(userProfile, "OneDrive", "Training", "Generative AI Foundations", "Questions");

var sourceImagePaths = Directory.GetFiles(folder, "*.png");

await using var writer = new StreamWriter(Path.Combine(folder, "output.md"));

foreach (var sourceImagePath in sourceImagePaths)
{
    Console.WriteLine(sourceImagePath);
    
    var questionWithAnswers = await analyzer.AnalyzeImageAsync(sourceImagePath);
    writer.WriteLine("### QUESTION:\r\n\r\n" + questionWithAnswers);
    writer.WriteLine();

    var answer = await analyzer.AnswerQuestionAsync(questionWithAnswers);
    writer.WriteLine("### ANSWER:\r\n\r\n" + answer);
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
            Endpoint = new Uri("http://127.0.0.1:1234/v1")
        }));

    services.AddSingleton<OlmOcrQuestionAndAnswerService>();

    return services.BuildServiceProvider();
}