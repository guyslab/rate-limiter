using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RateLimiter;

public sealed class MultiRateLimiter<TArg, TResult>: IMultiRateLimiter<TArg, TResult>
{
    private readonly Func<TArg, Task<TResult>> _operation;
    private readonly List<RateLimitTracker> _trackers;
    private readonly object _syncLock = new object();

    public MultiRateLimiter(Func<TArg, Task<TResult>> operation, params RateLimit[] rateLimits)
    {
        _operation = operation ?? throw new ArgumentNullException(nameof(operation));
        if (rateLimits == null || rateLimits.Length == 0)
            throw new ArgumentException("At least one RateLimit must be specified.", nameof(rateLimits));

        // Create a tracker for each rate limit
        _trackers = new List<RateLimitTracker>(rateLimits.Length);
        foreach (var rateLimit in rateLimits)
            _trackers.Add(new RateLimitTracker(rateLimit));
    }

    public async Task<TResult> Perform(TArg argument)
    {
        DateTime now;
        DateTime earliestAllowedTime;

        // Check each tracker and figure out the earliest time we are allowed to proceed
        lock (_syncLock)
        {
            now = DateTime.UtcNow;
            earliestAllowedTime = _trackers
                .Select(t => t.GetNextAvailableTime(now))
                .Max(); // must satisfy the latest requirement among all limits
        }

        // If must wait - delay
        if (earliestAllowedTime > now)
            await Task.Delay(earliestAllowedTime - now);

        // Consider a new time for the operation, whether we waited or not
        lock (_syncLock)
        {
            var currentTime = DateTime.UtcNow;
            foreach (var tracker in _trackers)
                tracker.RecordCall(currentTime);
        }

        return await _operation(argument);
    }
}
