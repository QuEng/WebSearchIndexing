using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace WebSearchIndexing.Extensions;

public static class ConfigurationExtension
{
    private static readonly string _appSettingsPath = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");

    public static string GetDbConnectionString(this IConfiguration configuration, string name = "DefaultConnection")
    {
        return configuration.GetValue<string>("ConnectionStrings:" + name) ?? throw new InvalidOperationException($"Connection string '{name}' not found.");
    }

    public static void SetConnectionString(this IConfiguration configuration, string value, string name = "DefaultConnection")
    {
        JObject appSettingsJson = ReadAppSettingsJson();
        appSettingsJson["ConnectionStrings"]![name] = value;
        WriteAppSettingsJson(appSettingsJson);
    }

    public static bool GetInitializedState(this IConfiguration configuration)
    {
        return configuration.GetValue<bool>("IsInitialized");
    }

    public static void SetInitializedState(this IConfiguration configuration, bool value)
    {
        JObject appSettingsJson = ReadAppSettingsJson();
        appSettingsJson["IsInitialized"] = value.ToString();
        WriteAppSettingsJson(appSettingsJson);
    }
    
    private static JObject ReadAppSettingsJson()
    {
        using (StreamReader reader = new StreamReader(_appSettingsPath))
        {
            string json = reader.ReadToEnd();
            return JObject.Parse(json);
        }
    }

    private static void WriteAppSettingsJson(JObject appSettingsJson)
    {
        File.WriteAllText(_appSettingsPath, appSettingsJson.ToString(Formatting.Indented));
    }
}