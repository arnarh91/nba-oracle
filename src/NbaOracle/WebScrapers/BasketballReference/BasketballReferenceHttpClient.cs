using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace NbaOracle.WebScrapers.BasketballReference;

public class BasketballReferenceHttpClient
{
    private readonly HttpClient _httpClient;

    public BasketballReferenceHttpClient(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    public async Task<string> Get(string requestUri)
    {
        var response = await _httpClient.GetAsync(requestUri);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }
}