using FreeGency.AI.ReviewModeration.Enums;

namespace FreeGency.AI.ReviewModeration.Models;

/// <summary>
/// The deterministic spam analysis of a review: the triggered signals and a
/// 0..100 score contribution that can raise the effective risk of the review.
/// </summary>
/// <param name="Signals">The triggered spam signals. Empty when no spam was found.</param>
/// <param name="Score">The deterministic spam score contribution on a 0..100 scale.</param>
public sealed record ReviewSpamResult(IReadOnlyList<ReviewSpamSignal> Signals, double Score);
