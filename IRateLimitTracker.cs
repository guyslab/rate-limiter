using System;

namespace RateLimiter;

internal interface IRateLimitTracker
{
    /// <summary>
    /// Returns the earliest time we are allowed to proceed with a new call, given the recent calls and this rate limit's window.
    /// </summary>
    DateTime GetNextAvailableTime(DateTime now);

    // Once it’s confirmed we can proceed, record the actual call time.
    void RecordCall(DateTime callTime);
}
