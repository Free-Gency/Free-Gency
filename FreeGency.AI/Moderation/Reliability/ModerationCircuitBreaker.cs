namespace FreeGency.AI.Moderation.Reliability;

/// <summary>
/// The current state of the circuit breaker.
/// </summary>
public enum CircuitBreakerState
{
    /// <summary>Calls are allowed through normally.</summary>
    Closed = 0,

    /// <summary>Calls are short-circuited to manual review until the reset timeout elapses.</summary>
    Open = 1,

    /// <summary>A single trial call is allowed to probe the AI service.</summary>
    HalfOpen = 2
}

/// <summary>
/// Thread-safe circuit breaker for the moderation pipeline. When more than
/// <c>failureThreshold</c> consecutive failures occur, the circuit opens and
/// calls short-circuit until <c>resetTimeout</c> elapses; the next call then
/// runs as a half-open trial that either closes the circuit on success or
/// reopens it on failure. All state is lock-guarded (never static).
/// </summary>
public sealed class ModerationCircuitBreaker
{
    private readonly object _sync = new();
    private readonly int _failureThreshold;
    private readonly TimeSpan _resetTimeout;
    private CircuitBreakerState _state = CircuitBreakerState.Closed;
    private int _consecutiveFailures;
    private DateTimeOffset _openedAt;

    public ModerationCircuitBreaker(int failureThreshold, TimeSpan resetTimeout)
    {
        if (failureThreshold <= 0)
            throw new ArgumentOutOfRangeException(nameof(failureThreshold), "The failure threshold must be positive.");

        _failureThreshold = failureThreshold;
        _resetTimeout = resetTimeout;
    }

    /// <summary>Gets the current circuit state.</summary>
    public CircuitBreakerState State
    {
        get
        {
            lock (_sync)
            {
                TryTransitionToHalfOpen();
                return _state;
            }
        }
    }

    /// <summary>
    /// Gets a value indicating whether calls should short-circuit. Reading this
    /// also advances an open circuit to half-open once the reset timeout elapses.
    /// </summary>
    public bool IsOpen
    {
        get
        {
            lock (_sync)
            {
                TryTransitionToHalfOpen();
                return _state == CircuitBreakerState.Open;
            }
        }
    }

    /// <summary>Records a successful call, closing the circuit and resetting the failure count.</summary>
    public void RecordSuccess()
    {
        lock (_sync)
        {
            _state = CircuitBreakerState.Closed;
            _consecutiveFailures = 0;
        }
    }

    /// <summary>
    /// Records a failed call. Opens the circuit when the consecutive failure
    /// threshold is reached or when a half-open trial fails.
    /// </summary>
    public void RecordFailure()
    {
        lock (_sync)
        {
            _consecutiveFailures++;

            if (_state == CircuitBreakerState.HalfOpen || _consecutiveFailures >= _failureThreshold)
            {
                _state = CircuitBreakerState.Open;
                _openedAt = DateTimeOffset.UtcNow;
            }
        }
    }

    private void TryTransitionToHalfOpen()
    {
        if (_state == CircuitBreakerState.Open && DateTimeOffset.UtcNow - _openedAt >= _resetTimeout)
            _state = CircuitBreakerState.HalfOpen;
    }
}
