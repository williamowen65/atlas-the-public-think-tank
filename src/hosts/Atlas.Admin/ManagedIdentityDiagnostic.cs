using System.Diagnostics;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace Atlas.Admin;

/// <summary>Read-only token acquisition probe in the actual WebJob process.</summary>
internal static class ManagedIdentityDiagnostic
{
    public static async Task RunAsync(IConfiguration configuration)
    {
        // Diagnostic for the 2026-11-05 schema token-acquisition timeout, not a
        // workaround or proof of a SQL connection fix. No SQL commands are run.
        // REST contract: https://learn.microsoft.com/azure/app-service/overview-managed-identity
        var endpoint = configuration["IDENTITY_ENDPOINT"];
        var header = configuration["IDENTITY_HEADER"];
        Console.WriteLine($"Managed identity diagnostic: IDENTITY_ENDPOINT present={!string.IsNullOrWhiteSpace(endpoint)}; IDENTITY_HEADER present={!string.IsNullOrWhiteSpace(header)}.");
        if (string.IsNullOrWhiteSpace(endpoint) || string.IsNullOrWhiteSpace(header))
            throw new InvalidOperationException("Managed identity endpoint settings are missing in the WebJob process. No SQL commands ran.");
        if (!Uri.TryCreate(endpoint, UriKind.Absolute, out var uri)
            || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
            || !string.IsNullOrEmpty(uri.UserInfo) || !string.IsNullOrEmpty(uri.Fragment))
            throw new InvalidOperationException("Managed identity endpoint setting is invalid; its value is not logged.");

        var clientId = Guid.Parse(configuration["ATLAS_ADMIN_MI_CLIENT_ID"]!);
        var separator = string.IsNullOrEmpty(uri.Query) ? "?" : "&";
        var requestUri = uri.AbsoluteUri + separator
            + "api-version=2019-08-01&resource=" + Uri.EscapeDataString("https://database.windows.net/")
            + "&client_id=" + clientId.ToString("D");
        // Never send the platform secret through a proxy or follow a redirect.
        using var handler = new HttpClientHandler { UseProxy = false, AllowAutoRedirect = false };
        using var client = new HttpClient(handler);
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        using var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
        request.Headers.Add("X-IDENTITY-HEADER", header);
        Console.WriteLine("Managed identity diagnostic: requesting SQL token for the configured admin client ID; timeout=30 seconds. Endpoint and credentials are not logged.");
        var timer = Stopwatch.StartNew();
        try
        {
            using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, timeout.Token);
            Console.WriteLine($"Managed identity endpoint HTTP status={(int)response.StatusCode}; elapsed={timer.ElapsedMilliseconds} ms.");
            if (!response.IsSuccessStatusCode)
                throw new InvalidOperationException("Managed identity token endpoint rejected the request. Response body withheld; no SQL commands ran.");
            using var body = await response.Content.ReadAsStreamAsync(timeout.Token);
            using var payload = await JsonDocument.ParseAsync(body, cancellationToken: timeout.Token);
            if (!payload.RootElement.TryGetProperty("access_token", out var token)
                || token.ValueKind != JsonValueKind.String || string.IsNullOrWhiteSpace(token.GetString()))
                throw new InvalidOperationException("Managed identity endpoint returned no usable access token. Response body withheld; no SQL commands ran.");
            Console.WriteLine("Managed identity SQL token acquisition succeeded. Token and response body withheld; no SQL connection or database changes performed.");
        }
        catch (OperationCanceledException)
        {
            // Do not propagate request exceptions: they can include endpoint URLs.
            throw new InvalidOperationException("Managed identity token request timed out after 30 seconds. No SQL commands ran.");
        }
        catch (HttpRequestException)
        {
            throw new InvalidOperationException("Managed identity token endpoint transport failed. Endpoint and headers withheld; no SQL commands ran.");
        }
        catch (JsonException)
        {
            throw new InvalidOperationException("Managed identity endpoint returned invalid JSON. Response body withheld; no SQL commands ran.");
        }
    }
}
