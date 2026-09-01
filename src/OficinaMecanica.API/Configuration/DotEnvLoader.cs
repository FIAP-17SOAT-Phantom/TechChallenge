namespace OficinaMecanica.API.Configuration;

public static class DotEnvLoader
{
    private static readonly Dictionary<string, string> ConfigurationKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        ["JWT_SECRET"] = "Jwt__Secret",
        ["ADMIN_EMAIL"] = "Authentication__SeedUsers__0__Email",
        ["ADMIN_PASSWORD"] = "Authentication__SeedUsers__0__Password"
    };

    public static void Load()
    {
        var currentDirectory = Directory.GetCurrentDirectory();
        var candidates = new[] { Path.Combine(currentDirectory, ".env"), Path.GetFullPath(Path.Combine(currentDirectory, "..", "..", ".env")) };
        var path = candidates.FirstOrDefault(File.Exists);

        if (path is null)
        {
            return;
        }

        foreach (var line in File.ReadLines(path))
        {
            var content = line.Trim();

            if (string.IsNullOrWhiteSpace(content) || content.StartsWith('#'))
            {
                continue;
            }

            var separatorIndex = content.IndexOf('=');

            if (separatorIndex <= 0)
            {
                continue;
            }

            var key = content[..separatorIndex].Trim();
            var value = content[(separatorIndex + 1)..].Trim().Trim('"', '\'');
            SetIfMissing(key, value);

            if (ConfigurationKeys.TryGetValue(key, out var configurationKey))
            {
                SetIfMissing(configurationKey, value);
            }
        }

        SetConnectionStringIfMissing();
    }

    private static void SetIfMissing(string key, string value)
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(key)))
        {
            Environment.SetEnvironmentVariable(key, value);
        }
    }

    private static void SetConnectionStringIfMissing()
    {
        const string connectionStringKey = "ConnectionStrings__DefaultConnection";

        if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(connectionStringKey)))
        {
            return;
        }

        var database = Environment.GetEnvironmentVariable("POSTGRES_DB");
        var username = Environment.GetEnvironmentVariable("POSTGRES_USER");
        var password = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD");

        if (!string.IsNullOrWhiteSpace(database) && !string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(password))
        {
            Environment.SetEnvironmentVariable(connectionStringKey, $"Host=localhost;Database={database};Username={username};Password={password}");
        }
    }
}
