using FreeGency.AI.ReviewModeration.Enums;

namespace FreeGency.AI.ReviewModeration.Models;

/// <summary>
/// A single deterministic security finding, such as a detected email address or
/// a sensitive value inside a review.
/// </summary>
/// <param name="Name">The finding name (for example "Email", "Phone", "CreditCard", "Wallet").</param>
/// <param name="Category">The security category the finding belongs to.</param>
public sealed record ReviewSensitiveMatch(string Name, ReviewSecurityCategory Category);
