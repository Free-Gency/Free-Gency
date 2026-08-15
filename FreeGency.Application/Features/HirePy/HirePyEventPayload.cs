namespace FreeGency.Application.Features.HirePy;

public sealed record HirePyEventPayload(
    Guid SessionId,
    string Status,
    string Stage,
    Guid? ProjectId = null,
    int? CandidateCount = null,
    string? Message = null);
