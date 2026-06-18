using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace RepoTraducaoPTBR;

internal static class CosmeticDumper
{
    public static void Dump(string dictionariesDir)
    {
        try
        {
            var type = AccessTools.TypeByName("CosmeticAsset");
            var nameField = type?.GetField("assetName");
            if (type == null || nameField == null)
            {
                BceConsole.LogWarning("CosmeticAsset não encontrado — dump cancelado.");
                return;
            }

            var names = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var asset in Resources.FindObjectsOfTypeAll(type))
            {
                if (nameField.GetValue(asset) is string n && !string.IsNullOrWhiteSpace(n))
                    names.Add(n);
            }

            var known = new HashSet<string>(StringComparer.Ordinal);
            foreach (var file in Directory.GetFiles(dictionariesDir, "*.txt"))
                foreach (var line in File.ReadAllLines(file))
                {
                    if (line.StartsWith("//") || string.IsNullOrWhiteSpace(line)) continue;
                    int eq = line.IndexOf('=');
                    if (eq > 0) known.Add(line.Substring(0, eq).Replace("\\=", "="));
                }

            var missing = names.Where(n => !known.Contains(n)).ToList();
            string outPath = Path.Combine(Paths.ConfigPath, "Translation", "pt", "RTLC_dump_cosmeticos.txt");
            Directory.CreateDirectory(Path.GetDirectoryName(outPath)!);
            File.WriteAllLines(outPath, new[]
            {
                "// Cosmetics carregados SEM tradução nos dicionários — gerado pelo botão Dump Cosmetic Names.",
                "// Preencha a parte direita e mova cada linha para o dicionário da categoria certa",
                $"// (plugins/.../Dictionaries/*.txt). Total carregado: {names.Count}; sem tradução: {missing.Count}.",
                "",
            }.Concat(missing.Select(n => $"{n}={n}")));

            BceConsole.LogInfo($"Dump de cosmetics: {names.Count} carregados, {missing.Count} sem tradução → {outPath}");
        }
        catch (Exception ex)
        {
            BceConsole.LogWarning($"Falha no dump de cosmetics: {ex.Message}");
        }
    }
}
