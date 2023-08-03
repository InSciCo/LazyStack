using System.Reflection;
using System.Xml.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LazyStackConfig;

public static class LzConfig
{
    /// <summary>
    /// Reads and merges JSON configuration files embedded in the specified assembly's manifest.
    /// </summary>
    /// <remarks>
    /// The method retrieves a list of JSON configuration files from the config.xml file. It respects 
    /// the order of these files as specified in config.xml during loading. Each JSON configuration file
    /// is then merged into a single configuration, with properties from later files overwriting 
    /// those from earlier ones. This allows for default value specification and override 
    /// based on the loading order.
    /// If no config.xml file exists, then all JSON configuration files are loaded in the order
    /// they are returned from the GetMainfestResourceNames() function.
    /// </remarks>
    /// <param name="assembly"></param>
    /// <param name="configResourcePath">
    /// The dot-separated path to the resources. Default is "Config.EmbeddedByBuild."
    /// </param>
    /// <param name="nameSpace">
    /// Namespace of the assembly. Default value is $"{assembly.GetName().Name}.". In rare circumstances
    /// you may need to override it by passing a value.
    /// </param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static string? ReadEmbeddedConfig(Assembly assembly, string configResourcePath = "Config.EmbeddedByBuild.", string? nameSpace = null)
    {
        nameSpace ??= assembly.GetName().Name + ".";
        var fullConfigResourcePath = nameSpace + configResourcePath;
        try
        {
            JObject? configMerge = null;
            string? configJson = null;

            var resourceNames = assembly.GetManifestResourceNames();
            if(resourceNames.Contains(fullConfigResourcePath + "config.xml"))
            {
                var configDoc = ReadEmbeddedConfigXml(assembly, $"{fullConfigResourcePath}config.xml");
                configMerge = configDoc.Descendants("Resource").Select(r => r.Value)
                    .Aggregate(configMerge, (currentConfig, name) =>
                        MergeJObject(
                            currentConfig,
                            ReadEmbeddedConfigJson(assembly, $"{fullConfigResourcePath}{name}")));

            } else
            {
                configMerge = resourceNames.Where(x => x.StartsWith(fullConfigResourcePath, StringComparison.OrdinalIgnoreCase))
                    .Aggregate(configMerge,(currentConfig, name) =>
                        MergeJObject(
                            currentConfig,
                            ReadEmbeddedConfigJson(assembly, name)));
               
            }

            return (configMerge != null)
                ? configJson = JsonConvert.SerializeObject(configMerge)
                : configJson;
        }
        catch (Exception ex)
        {
            throw new Exception($"Client Config error. {ex.Message}");
        }
    }
    private static XDocument ReadEmbeddedConfigXml(Assembly assembly, string resourcePath)
    {
        using var envFile = GetManifestResourceStreamSafe(assembly, resourcePath);
        using var envRead = new StreamReader(envFile);
        return XDocument.Parse(envRead.ReadToEnd());
    }
    private static JObject ReadEmbeddedConfigJson(Assembly assembly, string resourcePath) 
    {
        using var fileStream = GetManifestResourceStreamSafe(assembly, $"{resourcePath}")!;
        using var fileReader = new StreamReader(fileStream);
        var result = JObject.Parse(fileReader.ReadToEnd());
        return result;
    }     
    private static JObject MergeJObject(JObject? currentJObject, JObject newJObject) 
    {
        if (currentJObject is null) 
            return newJObject;
        currentJObject.Merge(newJObject, new JsonMergeSettings { MergeArrayHandling = MergeArrayHandling.Union });
        return currentJObject;
    }
    private static Stream GetManifestResourceStreamSafe(Assembly assembly, string resourceName)
    {
        var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream != null) return stream;
        throw new InvalidOperationException($"The resource '{resourceName}' is missing in the assembly '{assembly.FullName}'.");
    }

}