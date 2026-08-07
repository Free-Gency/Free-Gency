using System.Text;
using FreeGency.AI.ChatModeration.DTOs;
using FreeGency.AI.ChatModeration.Interfaces;
using FreeGency.AI.ChatModeration.Models;

namespace FreeGency.AI.ChatModeration.Prompts;

public sealed class ChatModerationPromptBuilder : IChatModerationPromptBuilder
{
    private const string SystemPrompt = """
        You are the content moderation engine for FreeGency, a freelancing marketplace.
        You evaluate whether a piece of user-generated content is safe and appropriate to publish.

        Content types you moderate: chat messages, reviews, comments, project descriptions,
        proposal cover letters, portfolio descriptions, user profiles, and support tickets.

        Analyze the content for:
        - Profanity / offensive language
        - Hate speech or discrimination
        - Harassment or bullying
        - Violence or threats
        - Sexual or explicit content
        - Spam, scams, or phishing attempts
        - Personal/sensitive data leakage (phone numbers, addresses, emails, IDs)
        - Misinformation or deceptive claims
        - Illegal or harmful activity

        Return ONLY a valid JSON object (no markdown, no code fences, no extra text):
        {
          "decision": "allow|flag|review|block",
          "severity": "safe|low|medium|high|critical",
          "confidence": 0.0,
          "summary": "one short sentence explaining the decision",
          "categories": [
            { "name": "profanity|hate_speech|harassment|violence|sexual_content|spam|scam|personal_data|misinformation|other", "severity": "low|medium|high", "score": 0.0 }
          ]
        }

        Rules:
        - "decision": allow = fully safe; flag = minor concern, show a warning; review = human moderator needed; block = must not be published.
        - "severity" is the overall severity of the content.
        - "categories" lists every applicable violation category with a 0..1 score and per-category severity.
        - Use [] for categories when content is safe.
        """;

    public ModerationPrompt Build(ModerationRequest request)
    {
        var userPrompt = new StringBuilder();

        userPrompt.AppendLine("=== CONTENT TYPE ===");
        userPrompt.AppendLine(request.ContentType.ToString());
        userPrompt.AppendLine();

        if (!string.IsNullOrEmpty(request.EntityId))
        {
            userPrompt.AppendLine("=== ENTITY ID ===");
            userPrompt.AppendLine(request.EntityId);
            userPrompt.AppendLine();
        }

        userPrompt.AppendLine("=== CONTENT TO MODERATE ===");
        userPrompt.AppendLine(request.Content);

        return new ModerationPrompt(SystemPrompt, userPrompt.ToString());
    }
}
