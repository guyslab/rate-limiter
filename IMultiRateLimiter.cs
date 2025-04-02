using System.Threading.Tasks;
namespace RateLimiter;

public interface IMultiRateLimiter<TArg, TResult>
{
    /// <summary>
    /// Performs an operation while respecting all configured rate limits.
    /// This method is thread-safe.
    /// </summary>
    public Task<TResult> Perform(TArg argument);

}
