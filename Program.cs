using Microsoft.Extensions.DependencyInjection;

namespace RateLimiter;

public class Program
{
    public static async Task Main(string[] args)
    {
        // Create a new service collection
        var serviceCollection = new ServiceCollection();
        
        serviceCollection.AddTransient<IRateLimiterTest, RateLimiterTest>();
        var serviceProvider = serviceCollection.BuildServiceProvider();
        var test = serviceProvider.GetService<IRateLimiterTest>();
        if (test != null)
            await test.Run();
    }
}

