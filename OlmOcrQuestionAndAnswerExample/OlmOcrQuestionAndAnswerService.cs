using System.Text.Json;
using Microsoft.Extensions.Logging;
using OlmOcrExample.Models;
using OpenAI;
using OpenAI.Chat;
using SkiaSharp;

namespace OlmOcrExample;

internal class OlmOcrQuestionAndAnswerService(ILogger<OlmOcrQuestionAndAnswerService> logger, OpenAIClient openAIClient)
{
    private const string Model = "olmocr-7b-0225-preview";

    public async Task<string> AnalyzeImageAsync(string sourceImagePath, CancellationToken cancellationToken = default)
    {
        var (imageBytes, mimeType, dimensions, image) = await LoadImageDetailsAsync(sourceImagePath, cancellationToken);

        var systemChatMessage = new SystemChatMessage(Prompts.OcrSystemPrompt);

        var userChatMessage = new UserChatMessage
        (
            ChatMessageContentPart.CreateTextPart(Prompts.BuildPrompt(dimensions, image)),
            ChatMessageContentPart.CreateImagePart(BinaryData.FromBytes(imageBytes), mimeType)
        );

        var content = await CompleteChatAsync([systemChatMessage, userChatMessage], cancellationToken);

        return Sanitize(TryDeserialize(content));
    }

    public async Task<string> AnswerQuestionAsync(string questionWithAnswers, CancellationToken cancellationToken = default)
    {
        var systemChatMessage = new SystemChatMessage(Prompts.AnswerSystemPrompt);

        var userChatMessage = new UserChatMessage($"Answer this question using the provided answers: {questionWithAnswers}");
        return Sanitize(await CompleteChatAsync([systemChatMessage, userChatMessage], cancellationToken));
    }

    private static string Sanitize(string text)
    {
        return text
            .Replace("\\n", "\r\n")
            .Replace("’", "'")
            .Replace("“", "\"")
            .Replace("”", "\"");
    }

    private static async Task<(byte[] ImageBytes, string MimeType, Dimensions Dimensions, SKRect Image)> LoadImageDetailsAsync(string sourceImagePath, CancellationToken cancellationToken)
    {
        var imageBytes = await File.ReadAllBytesAsync(sourceImagePath, cancellationToken);

        var (resizedImage, dimensions, image) = ImageResizer.Resize(imageBytes);
        return (resizedImage, "image/png", dimensions, image);
    }

    private async Task<string> CompleteChatAsync(IEnumerable<ChatMessage> messages, CancellationToken cancellationToken = default)
    {
        var chatClient = openAIClient.GetChatClient(Model);

        var response = await chatClient.CompleteChatAsync(messages, cancellationToken: cancellationToken);
        return string.Concat(response.Value.Content.Select(c => c.Text)).Trim();
    }

    private string TryDeserialize(string content)
    {
        if (content.StartsWith('{'))
        {
            try
            {
                var olmResponse = JsonSerializer.Deserialize<OlmResponse>(content);
                if (olmResponse != null)
                {
                    return olmResponse.NaturalText;
                }
            }
            catch (JsonException)
            {
                logger.LogInformation("Failed to deserialize as {Response}.", nameof(OlmResponse));
            }
        }

        return content;
    }
}