using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace NbaOracle.WebScrapers.Espn;

public class EspnHttpClient
{
    private readonly HttpClient _httpClient;

    public EspnHttpClient(HttpClient httpClient)
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