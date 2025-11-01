using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace NbaOracle.Infrastructure.Http;

public class RateLimitingHandler : DelegatingHandler
{
    private readonly TimeSpan _minInterval;
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private DateTime _lastRequestTime = DateTime.MinValue;

    public RateLimitingHandler(TimeSpan minInterval)
    {
        _minInterval = minInterval;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            var timeSinceLastRequest = DateTime.UtcNow - _lastRequestTime;
            var delay = _minInterval - timeSinceLastRequest;

            if (delay > TimeSpan.Zero)
            {
                await Task.Delay(delay, cancellationToken);
            }

            _lastRequestTime = DateTime.UtcNow;
            return await base.SendAsync(request, cancellationToken);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _semaphore?.Dispose();
        }
        base.Dispose(disposing);
    }
}