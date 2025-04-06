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
        DateTime callTime;
        
        await Task.Delay(GetDelayTimeAndRecordCall(out callTime));
        
        return await _operation(argument);
    }

    private TimeSpan GetDelayTimeAndRecordCall(out DateTime callTime)
    {
        lock (_syncLock)
        {
            var now = DateTime.UtcNow;
            
            // Get the earliest time we're allowed to proceed based on all trackers
            var earliestAllowedTime = _trackers
                .Select(t => t.GetNextAvailableTime(now))
                .Max();
            
            TimeSpan delay = TimeSpan.Zero;
            if (earliestAllowedTime > now)
            {
                delay = earliestAllowedTime - now;
                // record the call at the future time when it will actually execute
                callTime = earliestAllowedTime;
            }
            else
            {
                // proceed immediately
                callTime = now;
            }
            
            foreach (var tracker in _trackers)
                tracker.RecordCall(callTime);
                
            return delay;
        }
    }
}
