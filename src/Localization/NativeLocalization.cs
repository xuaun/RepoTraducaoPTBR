using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;

namespace RepoTraducaoPTBR;

internal static class NativeLocalization
{
    private static readonly string[] Tables = { "HUD", "Menu", "Game" };

    private static string _tsvDir = null!;
    private static Func<bool> _enabled = null!;
    private static bool _installed;

    private static readonly Dictionary<(string Table, string Key), string?> _originals = new();
    private static bool _applied;

    internal static void Install(string tsvDir, Func<bool> enabled, Harmony harmony)
    {
        _tsvDir = tsvDir;
        _enabled = enabled;

        var target = AccessTools.Method("LocalizationManager:LoadLocalizationOverrides");
        if (target == null)
        {
            BceConsole.LogWarning("LocalizationManager.LoadLocalizationOverrides não encontrado — tradução da interface vanilla desativada (versão do jogo incompatível?).");
            return;
        }
        harmony.Patch(target, postfix: new HarmonyMethod(typeof(NativeLocalization), nameof(AfterVanillaOverrides)));
        _installed = true;
    }

    private static void AfterVanillaOverrides()
    {
        if (_enabled()) Apply();
    }

    internal static void OnToggleChanged(bool enabled)
    {
        if (!_installed) return;
        try
        {
            if (!LocalizationSettings.HasSettings) return;
            if (enabled) Apply(); else Restore();
            NotifyGame();
        }
        catch (Exception ex)
        {
            BceConsole.LogWarning($"Falha ao alternar a tradução: {ex.Message}");
        }
    }

    private static void Apply()
    {
        if (_applied) return;
        int applied = 0;
        try
        {
            foreach (string tableName in Tables)
            {
                string path = Path.Combine(_tsvDir, tableName + ".tsv");
                if (!File.Exists(path)) continue;

                StringTable? table = LocalizationSettings.StringDatabase.GetTable(tableName, LocalizationSettings.ProjectLocale);
                if (table == null)
                {
                    BceConsole.LogWarning($"Tabela de localização '{tableName}' não encontrada — pulando.");
                    continue;
                }

                foreach (string line in File.ReadAllLines(path))
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    string[] parts = line.Split('\t');
                    if (parts.Length != 2) continue;

                    string key = parts[0];
                    string value = parts[1].Replace("\\n", "\n");

                    var entry = table.GetEntry(key);
                    if (entry != null)
                    {
                        _originals[(tableName, key)] = entry.Value;
                        entry.Value = value;
                    }
                    else
                    {
                        _originals[(tableName, key)] = null;
                        table.AddEntry(key, value);
                    }
                    applied++;
                }
            }
            _applied = true;
            BceConsole.LogInfo($"Interface vanilla traduzida ({applied} textos nas tabelas {string.Join("/", Tables)}).");
        }
        catch (Exception ex)
        {
            BceConsole.LogWarning($"Falha ao aplicar a tradução: {ex.Message}");
        }
    }

    private static void Restore()
    {
        if (!_applied) return;
        try
        {
            foreach (var pair in _originals)
            {
                StringTable? table = LocalizationSettings.StringDatabase.GetTable(pair.Key.Table, LocalizationSettings.ProjectLocale);
                if (table == null) continue;
                if (pair.Value != null)
                {
                    var entry = table.GetEntry(pair.Key.Key);
                    if (entry != null) entry.Value = pair.Value;
                }
                else table.RemoveEntry(pair.Key.Key);
            }
            _originals.Clear();
            _applied = false;
            BceConsole.LogInfo("Tradução da interface vanilla desativada (textos originais restaurados).");
        }
        catch (Exception ex)
        {
            BceConsole.LogWarning($"Falha ao restaurar os textos originais: {ex.Message}");
        }
    }

    private static void NotifyGame()
    {
        var managerType = AccessTools.TypeByName("LocalizationManager");
        var instance = managerType?.GetField("instance")?.GetValue(null);
        if (instance == null) return;
        AccessTools.Method(managerType, "NotifyLocalizationChanged")?.Invoke(instance, null);
    }
}
