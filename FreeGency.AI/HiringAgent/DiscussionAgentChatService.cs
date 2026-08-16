using System.Text.Json;
using System.Text.RegularExpressions;
using FreeGency.AI.Prompts;
using Microsoft.SemanticKernel.ChatCompletion;

namespace FreeGency.AI.HiringAgent;

public sealed class DiscussionAgentChatService
{
    private readonly IChatCompletionService _chat;

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip
    };

    public DiscussionAgentChatService(IChatCompletionService chat)
    {
        _chat = chat;
    }

    public async Task<DiscussionAgentReply> GenerateReplyAsync(
        string projectContext,
        string freelancerLabel,
        string transcript,
        bool hasMilestonePlan,
        string? planStatus = null,
        string? planSummary = null,
        CancellationToken ct = default)
    {
        var userPayload = $"""
            PROJECT CONTEXT:
            {projectContext}

            FREELANCER:
            {freelancerLabel}

            HAS_MILESTONE_PLAN_IN_PRODUCT: {hasMilestonePlan}
            PLAN_STATUS: {planStatus ?? (hasMilestonePlan ? "Unknown" : "None")}
            LATEST_PLAN_SUMMARY:
            {planSummary ?? "(none)"}

            RECENT_CHAT_TRANSCRIPT:
            {transcript}

            Produce the next hiring-agent message for multi-candidate discussion.
            Do NOT request formal plan changes (requestPlanChanges must be false).
            {(hasMilestonePlan
                ? """
            A milestone plan already exists in the product.
            Thank them briefly. Do NOT ask to revise dates, amounts, milestones, deliverables, timeline, or budget.
            Do NOT ask them to update/change the plan in chat. Set doneNegotiating=true.
            """
                : "If no plan exists yet, nudge them to propose one in the product.")}
            """;

        var history = new ChatHistory();
        history.AddSystemMessage(PromptTemplates.HiringDiscussionAgent);
        history.AddUserMessage(userPayload);

        var response = await _chat.GetChatMessageContentsAsync(history, cancellationToken: ct);
        var raw = response.FirstOrDefault()?.Content ?? "{}";
        // Multi-candidate discussion never opens product change-requests.
        return Parse(raw, allowRequestPlanChanges: false);
    }

    /// <summary>
    /// Final review with the client-approved recommended candidate only.
    /// </summary>
    public async Task<DiscussionAgentReply> GenerateFinalPlanReviewAsync(
        string projectContext,
        string freelancerLabel,
        string transcript,
        bool hasMilestonePlan,
        string? planStatus = null,
        string? planSummary = null,
        CancellationToken ct = default)
    {
        var userPayload = $"""
            PROJECT CONTEXT:
            {projectContext}

            APPROVED FREELANCER:
            {freelancerLabel}

            HAS_MILESTONE_PLAN_IN_PRODUCT: {hasMilestonePlan}
            PLAN_STATUS: {planStatus ?? (hasMilestonePlan ? "Unknown" : "None")}
            LATEST_PLAN_SUMMARY:
            {planSummary ?? "(none)"}

            RECENT_CHAT_TRANSCRIPT:
            {transcript}

            The client approved/selected this candidate. Review the plan.
            If revisions are needed: requestPlanChanges=true and put ALL specific revision asks in changeComment
            (that becomes the formal Request Changes body). Keep message as a short acknowledgement only.
            If the plan is hire-ready: requestPlanChanges=false.
            """;

        var history = new ChatHistory();
        history.AddSystemMessage(PromptTemplates.HiringFinalPlanReview);
        history.AddUserMessage(userPayload);

        var response = await _chat.GetChatMessageContentsAsync(history, cancellationToken: ct);
        var raw = response.FirstOrDefault()?.Content ?? "{}";
        return Parse(raw, allowRequestPlanChanges: true);
    }

    internal static DiscussionAgentReply Parse(string raw, bool allowRequestPlanChanges = false)
    {
        try
        {
            var cleaned = StripJsonFences(raw);
            var json = ExtractJsonObject(cleaned) ?? cleaned;
            var parsed = JsonSerializer.Deserialize<DiscussionAgentReplyDto>(json, JsonOpts);
            if (parsed is null || string.IsNullOrWhiteSpace(parsed.Message))
            {
                return new DiscussionAgentReply
                {
                    Message =
                        "Thanks for joining. Could you outline a milestone plan with deliverables, amounts, and timing that fit this project's budget and timeline?",
                    DoneNegotiating = false
                };
            }

            var requestChanges = allowRequestPlanChanges && parsed.RequestPlanChanges;
            var changeComment = string.IsNullOrWhiteSpace(parsed.ChangeComment)
                ? null
                : parsed.ChangeComment.Trim();

            if (requestChanges && string.IsNullOrWhiteSpace(changeComment))
                changeComment = parsed.Message.Trim();

            return new DiscussionAgentReply
            {
                Message = parsed.Message.Trim(),
                DoneNegotiating = parsed.DoneNegotiating && !requestChanges,
                InternalNote = parsed.InternalNote,
                RequestPlanChanges = requestChanges,
                ChangeComment = changeComment
            };
        }
        catch
        {
            return new DiscussionAgentReply
            {
                Message =
                    "Thanks — please share a concrete milestone plan (deliverables, amounts, and due dates) so we can move forward.",
                DoneNegotiating = false
            };
        }
    }

    private sealed class DiscussionAgentReplyDto
    {
        public string Message { get; set; } = string.Empty;
        public bool DoneNegotiating { get; set; }
        public string? InternalNote { get; set; }
        public bool RequestPlanChanges { get; set; }
        public string? ChangeComment { get; set; }
    }

    private static string StripJsonFences(string raw)
    {
        var m = Regex.Match(raw, @"```(?:json)?\s*([\s\S]*?)```", RegexOptions.IgnoreCase);
        return m.Success ? m.Groups[1].Value.Trim() : raw.Trim();
    }

    private static string? ExtractJsonObject(string text)
    {
        var start = text.IndexOf('{');
        var end = text.LastIndexOf('}');
        if (start < 0 || end <= start) return null;
        return text[start..(end + 1)];
    }
}
