using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RepoTraducaoPTBR;

internal sealed class IniFile
{
    private readonly string _path;

    public IniFile(string path) => _path = path;

    public bool Set(string section, string key, string value)
    {
        try
        {
            var lines = File.Exists(_path)
                ? File.ReadAllLines(_path).ToList()
                : new List<string>();

            int sectionStart = lines.FindIndex(l => l.Trim() == $"[{section}]");
            if (sectionStart < 0)
            {
                if (lines.Count > 0 && lines[^1].Trim().Length > 0) lines.Add("");
                lines.Add($"[{section}]");
                lines.Add($"{key}={value}");
                File.WriteAllLines(_path, lines);
                return true;
            }

            int sectionEnd = lines.FindIndex(sectionStart + 1, l => l.TrimStart().StartsWith("["));
            if (sectionEnd < 0) sectionEnd = lines.Count;

            for (int i = sectionStart + 1; i < sectionEnd; i++)
            {
                var t = lines[i];
                int eq = t.IndexOf('=');
                if (eq <= 0 || !t.Substring(0, eq).Trim().Equals(key, StringComparison.OrdinalIgnoreCase))
                    continue;
                if (t.Substring(eq + 1).Trim() == value) return false;
                lines[i] = $"{key}={value}";
                File.WriteAllLines(_path, lines);
                return true;
            }

            lines.Insert(sectionEnd, $"{key}={value}");
            File.WriteAllLines(_path, lines);
            return true;
        }
        catch (Exception ex)
        {
            BceConsole.LogWarning($"Falha ao editar o AutoTranslatorConfig.ini ({section}.{key}): {ex.Message}");
            return false;
        }
    }
}
