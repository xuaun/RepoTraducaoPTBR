using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;

namespace RepoTraducaoPTBR;

internal static class Xuat
{
    private const BindingFlags Any = BindingFlags.Public | BindingFlags.NonPublic
                                   | BindingFlags.Instance | BindingFlags.Static;

    private static Assembly? CoreAssembly =>
        AppDomain.CurrentDomain.GetAssemblies()
            .FirstOrDefault(a => a.GetName().Name == "XUnity.AutoTranslator.Plugin.Core");

    private static Type? PluginType =>
        CoreAssembly?.GetType("XUnity.AutoTranslator.Plugin.Core.AutoTranslationPlugin");

    private static Type? SettingsType =>
        CoreAssembly?.GetType("XUnity.AutoTranslator.Plugin.Core.Configuration.Settings");

    private static object? Instance => PluginType?.GetField("Current", Any)?.GetValue(null);

    internal static bool IsLoaded => Instance != null;

    internal static string? ConfiguredEndpointId =>
        SettingsType?.GetField("ServiceEndpoint", Any)?.GetValue(null) as string;

    internal static string? AutoTranslationsFilePath =>
        SettingsType?.GetField("AutoTranslationsFilePath", Any)?.GetValue(null) as string;

    internal static bool TryApplyConfig((string Section, string Key, string Value)[] entries,
                                        out bool changed, out string? error)
    {
        changed = false;
        error = null;
        try
        {
            var envType = CoreAssembly?.GetType("XUnity.AutoTranslator.Plugin.Core.PluginEnvironment");
            var env = GetFieldOrProperty(envType, null, "Current");
            if (env == null) { error = "PluginEnvironment.Current não encontrado"; return false; }

            var prefs = GetFieldOrProperty(env.GetType(), env, "Preferences")
                ?? CoreAssembly?.GetType("XUnity.AutoTranslator.Plugin.Core.IPluginEnvironment")
                    ?.GetProperty("Preferences", Any)?.GetValue(env);
            if (prefs == null) { error = "PluginEnvironment.Preferences não encontrado"; return false; }

            var sectionIndexer = prefs.GetType().GetProperty("Item", new[] { typeof(string) });
            if (sectionIndexer == null) { error = "indexer do IniFile não encontrado"; return false; }

            foreach (var (section, key, value) in entries)
            {
                var sec = sectionIndexer.GetValue(prefs, new object[] { section });
                var keyIndexer = sec?.GetType().GetProperty("Item", new[] { typeof(string) });
                var iniKey = keyIndexer?.GetValue(sec, new object[] { key });
                var valueProp = iniKey?.GetType().GetProperty("Value", Any);
                if (valueProp == null) { error = $"IniKey.Value não encontrado ({section}.{key})"; return false; }

                if ((valueProp.GetValue(iniKey) as string) == value) continue;
                valueProp.SetValue(iniKey, value);
                changed = true;
            }

            if (!changed) return true;

            var configure = SettingsType?.GetMethod("Configure", Any, null, Type.EmptyTypes, null);
            if (configure == null) { error = "Settings.Configure não encontrado"; return false; }
            configure.Invoke(null, null);
            return true;
        }
        catch (Exception ex)
        {
            error = ex.InnerException?.Message ?? ex.Message;
            return false;
        }
    }

    internal static bool TryGetCurrentEndpointId(out string? id)
    {
        id = null;
        try
        {
            var tm = GetTranslationManager(out _);
            var current = tm?.GetType().GetProperty("CurrentEndpoint", Any)?.GetValue(tm);
            id = EndpointIdOf(current);
            return id != null;
        }
        catch { return false; }
    }

    internal static bool TrySetEndpoint(string endpointId, out string? error)
    {
        error = null;
        try
        {
            var instance = Instance;
            if (instance == null) { error = "AutoTranslator ainda não carregou"; return false; }

            var tm = GetTranslationManager(out error);
            if (tm == null) return false;

            if (tm.GetType().GetProperty("AllEndpoints", Any)?.GetValue(tm) is not IEnumerable all)
            { error = "AllEndpoints não encontrado"; return false; }

            object? target = null;
            foreach (var em in all)
                if (string.Equals(EndpointIdOf(em), endpointId, StringComparison.OrdinalIgnoreCase))
                { target = em; break; }
            if (target == null) { error = $"endpoint '{endpointId}' não existe"; return false; }

            if (target.GetType().GetProperty("Error", Any)?.GetValue(target) is Exception initError)
            { error = $"endpoint '{endpointId}' falhou ao inicializar: {initError.Message}"; return false; }

            var select = PluginType!.GetMethod("OnEndpointSelected", Any);
            if (select == null) { error = "OnEndpointSelected não encontrado"; return false; }

            select.Invoke(instance, new[] { target });
            return true;
        }
        catch (Exception ex)
        {
            error = ex.InnerException?.Message ?? ex.Message;
            return false;
        }
    }

    internal static void EnsureAutoGenFile(string? fallbackPath = null)
    {
        try
        {
            string? path = AutoTranslationsFilePath ?? fallbackPath;
            if (string.IsNullOrEmpty(path) || File.Exists(path)) return;
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, string.Empty);
        }
        catch { }
    }

    internal static bool TryReloadTranslations(out string? error)
    {
        error = null;
        try
        {
            var instance = Instance;
            if (instance == null) { error = "AutoTranslator ainda não carregou"; return false; }
            var reload = PluginType!.GetMethod("ReloadTranslations", Any, null, Type.EmptyTypes, null);
            if (reload == null) { error = "ReloadTranslations não encontrado"; return false; }

            EnsureAutoGenFile();

            reload.Invoke(instance, null);
            return true;
        }
        catch (Exception ex)
        {
            error = ex.InnerException?.Message ?? ex.Message;
            return false;
        }
    }

    private static object? GetFieldOrProperty(Type? type, object? obj, string name)
    {
        if (type == null) return null;
        if (type.GetField(name, Any) is FieldInfo f) return f.GetValue(obj);
        if (type.GetProperty(name, Any) is PropertyInfo p) return p.GetValue(obj);
        return null;
    }

    private static object? GetTranslationManager(out string? error)
    {
        error = null;
        var instance = Instance;
        if (instance == null) { error = "AutoTranslator ainda não carregou"; return null; }
        var tm = PluginType!.GetField("TranslationManager", Any)?.GetValue(instance);
        if (tm == null) error = "TranslationManager não encontrado";
        return tm;
    }

    private static string? EndpointIdOf(object? endpointManager)
    {
        if (endpointManager == null) return null;
        var ep = endpointManager.GetType().GetProperty("Endpoint", Any)?.GetValue(endpointManager);
        return ep?.GetType().GetProperty("Id", Any)?.GetValue(ep) as string;
    }
}
