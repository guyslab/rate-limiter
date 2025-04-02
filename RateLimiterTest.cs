using System.Threading.Tasks;
namespace RateLimiter;
public class RateLimiterTest: IRateLimiterTest
{
    public async Task Run()
    {
        // Test function simulating an API call
        async Task<string> ExternalApiCall(int x)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(200));
            return $"Result for {x}";
        }

        var limiter = new MultiRateLimiter<int, string>(
            ExternalApiCall,
            new RateLimit(10, TimeSpan.FromSeconds(5)),  // up to 10/5secs
            new RateLimit(100, TimeSpan.FromMinutes(1)), // up to 100/min
            new RateLimit(1000, TimeSpan.FromDays(1))    // up to 1000/day
        );

        var tasks = new List<Task>();
        for (int i = 0; i < 30; i++)
        {
            int copy = i;
            tasks.Add(Task.Run(async () =>
            {
                string result = await limiter.Perform(copy);
                Console.WriteLine($"Call {copy} => {result} at {DateTime.Now:HH:mm:ss.fff}");
            }));
        }

        await Task.WhenAll(tasks);
    }
}

