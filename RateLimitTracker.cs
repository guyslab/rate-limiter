using System;
using System.Collections.Generic;
using System.Linq;

namespace RateLimiter;

internal class RateLimitTracker: IRateLimitTracker
{
    private readonly RateLimit _rateLimit;
    private readonly Queue<DateTime> _recentCallTimestamps = new Queue<DateTime>();

    public RateLimitTracker(RateLimit rateLimit)
    {
        _rateLimit = rateLimit;
    }

    public DateTime GetNextAvailableTime(DateTime now)
    {
        // remove timestamps that are older than the sliding window
        while (_recentCallTimestamps.Count > 0 &&
               (now - _recentCallTimestamps.Peek()) >= _rateLimit.window)
        {
            _recentCallTimestamps.Dequeue();
        }

        // If window has space for the new call
        if (_recentCallTimestamps.Count <= _rateLimit.maxCount)
        {
            return now;
        }

        // The earliest call in the queue is the first that blocks us.
        // Once that first call becomes older than the window, we can proceed.
        // That time = (earliest call time + window).
        var earliestCallTime = _recentCallTimestamps.Peek();
        return earliestCallTime + _rateLimit.window;
    }

    public void RecordCall(DateTime callTime)
    {
        GetNextAvailableTime(callTime); // remove old timestamps
        _recentCallTimestamps.Enqueue(callTime);
    }
}
