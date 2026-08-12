using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace FreeGency.AI;

public class BedrockGatewayChatService : IChatCompletionService
{
    private readonly HttpClient _httpClient;
    private readonly string _modelId;

    public IReadOnlyDictionary<string, object?> Attributes => new Dictionary<string, object?>();

    public BedrockGatewayChatService(HttpClient httpClient, IConfiguration configuration)
    {
        var apiKey = configuration["SBG_API_KEY"]
                     ?? throw new InvalidOperationException("Missing SBG_API_KEY in appsettings");

        _modelId = configuration["AI:ChatModelId"]
                   ?? configuration["AI:DefaultModelId"]
                   ?? "google.gemma-3-27b-it";

        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("http://apiaccess.iti.net.eg");
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
    }

    public async Task<IReadOnlyList<ChatMessageContent>> GetChatMessageContentsAsync(
        ChatHistory chatHistory, PromptExecutionSettings? executionSettings = null,
        Kernel? kernel = null, CancellationToken cancellationToken = new CancellationToken())
    {
        var userMessage = chatHistory.LastOrDefault(m => m.Role == AuthorRole.User)?.Content ?? "";
        var systemMessage = chatHistory.FirstOrDefault(m => m.Role == AuthorRole.System)?.Content;

        var payload = new
        {
            model_id = _modelId,
            messages = new[]
            {
                new
                {
                    role = "user",
                    content = userMessage
                }
            },
            system_prompt = systemMessage,
            max_tokens = 1500
        };

        var json = JsonSerializer.Serialize(payload);
        var response = await _httpClient.PostAsync(
            "api/v1/student/chat",
            new StringContent(json, Encoding.UTF8, "application/json"),
            cancellationToken);

        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var detail = string.IsNullOrWhiteSpace(body) ? "(empty response body)" : body.Trim();
            var hint = response.StatusCode switch
            {
                HttpStatusCode.Unauthorized =>
                    "SBG_API_KEY is missing, invalid, or expired. Refresh it from ITI Student Bedrock Gateway.",
                HttpStatusCode.Forbidden =>
                    "ITI Bedrock Gateway returned 403 Forbidden. This is usually an access/key/quota issue — not an app bug. Confirm your SBG_API_KEY is active and that your account still has chat access.",
                HttpStatusCode.NotFound =>
                    $"Chat endpoint or model '{_modelId}' was not found. Check AI:ChatModelId against the models allowed for your key.",
                _ => $"ITI Bedrock Gateway error ({(int)response.StatusCode} {response.StatusCode})."
            };

            throw new InvalidOperationException($"{hint} Gateway detail: {detail}");
        }

        var reply = JsonDocument.Parse(body).RootElement.GetProperty("output_text").GetString() ?? "";
        return new List<ChatMessageContent> { new(AuthorRole.Assistant, reply) };
    }

    public IAsyncEnumerable<StreamingChatMessageContent> GetStreamingChatMessageContentsAsync(
        ChatHistory chatHistory,
        PromptExecutionSettings? executionSettings = null, Kernel? kernel = null,
        CancellationToken cancellationToken = new CancellationToken())
    {
        throw new NotImplementedException();
    }
}
