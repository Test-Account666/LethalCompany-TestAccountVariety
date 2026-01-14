using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx.Configuration;
using MonoMod.Utils;

namespace TestAccountVariety.Config;

internal abstract class VarietyConfig {
    public abstract void Initialize(ConfigFile configFile);

    public static void InitializeConfigs(ConfigFile configFile) {
        var types = new List<Type>();

        try {
            types.AddRange(Assembly.GetExecutingAssembly().GetTypes());
        } catch (ReflectionTypeLoadException exception) {
            types.AddRange(exception.Types.Where(t => t != null));
        }

        foreach (var type in types) {
            try {
                if (type == typeof(VarietyConfig)) continue;

                if (!type.IsSubclassOf(typeof(VarietyConfig))) continue;

                var config = Activator.CreateInstance(type) as VarietyConfig;

                config?.Initialize(configFile);
            } catch (Exception exception) {
                TestAccountVariety.Logger.LogWarning("You may be able to ignore this error!");
                exception.LogDetailed();
            }
        }
    }
}

public static class ConfigHelper {
    public static ConfigEntry<int> BindInt(this ConfigFile configFile, string section, string key, int defaultValue, string description,
        int min = 0,
        int max = 100) =>
        configFile.Bind(section, key, defaultValue, new ConfigDescription(description, new AcceptableValueRange<int>(min, max)));

    public static ConfigEntry<float> BindFloat(this ConfigFile configFile, string section, string key, float defaultValue,
        string description, float min = .1F,
        float max = 1F) =>
        configFile.Bind(section, key, defaultValue, new ConfigDescription(description, new AcceptableValueRange<float>(min, max)));

    public static ConfigEntry<bool> BindBool(this ConfigFile configFile, string section, string key, bool defaultValue,
        string description) =>
        configFile.Bind(section, key, defaultValue, description);

    public static ConfigEntry<string> BindString(this ConfigFile configFile, string section, string key, string defaultValue,
        string description) =>
        configFile.Bind(section, key, defaultValue, description);
}