namespace RateLimiter;

/// <summary>
/// Represents a limit of calls within a time window
/// </summary>
/// <param name="maxCount">Maximum number of calls allowed in the given window</param>
/// <param name="window">Time window (e.g. 1 day, 1 minute, etc.)</param>
public record RateLimit(    
    int maxCount,
    TimeSpan window);
