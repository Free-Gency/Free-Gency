using System.Text;
using System.Text.Json;
using System.Net.Http.Headers; 
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.Extensions.Configuration;

namespace FreeGency.AI;

public class BedrockGatewayChatService : IChatCompletionService
{
    private readonly HttpClient _httpClient;
    private const string ModelId = "meta.llama4-scout-17b-instruct-v1:0"; 
    
    public IReadOnlyDictionary<string, object?> Attributes => new Dictionary<string, object?>();
    
    public BedrockGatewayChatService(HttpClient httpClient, IConfiguration configuration)
    {
        var apiKey = configuration["SBG_API_KEY"]
                     ?? throw new InvalidOperationException("Missing SBG_API_KEY in appsettings");

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
            model_id = ModelId,
            messages = new[]
            {
                new { 
                    role = "user",
                    content = userMessage
                }
            },
            system_prompt = systemMessage,
            max_tokens = 1500
        };
        
        var json = JsonSerializer.Serialize(payload);
        var response = await _httpClient.PostAsync("api/v1/student/chat", 
            new StringContent(json, Encoding.UTF8, "application/json"), cancellationToken);
        
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        
        Console.WriteLine(body);
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