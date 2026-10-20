using System.Text.Json;

namespace Atlas.Api;

/// <summary>Development-only stand-in for account email delivery. Never returns tokens over HTTP.</summary>
public sealed class DevelopmentAccountMail(IWebHostEnvironment environment, IConfiguration configuration)
{
    public bool IsAvailable => environment.IsDevelopment();

    public async Task WriteAsync(string purpose, Guid userId, string token, CancellationToken cancellationToken)
    {
        if (!IsAvailable) throw new InvalidOperationException("Account email delivery is not configured.");
        var configured = configuration["Atlas:DevelopmentMailDirectory"];
        var directory = Path.GetFullPath(string.IsNullOrWhiteSpace(configured)
            ? "development-mail" : configured, environment.ContentRootPath);
        Directory.CreateDirectory(directory);
        await File.WriteAllTextAsync(Path.Combine(directory, Guid.NewGuid() + ".json"),
            JsonSerializer.Serialize(new { purpose, userId, token }), cancellationToken);
    }
}
