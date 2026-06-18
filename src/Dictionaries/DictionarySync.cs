using System;
using System.IO;
using System.Linq;

namespace RepoTraducaoPTBR;

internal sealed class DictionarySync
{
    private const string ManagedPrefix = "RTLC_pt_";

    private readonly string _dictionariesDir;
    private readonly string _textDir;
    private readonly string _autoGenPath;
    private readonly PluginConfig _config;

    public DictionarySync(string dictionariesDir, string textDir, string autoGenPath, PluginConfig config)
    {
        _dictionariesDir = dictionariesDir;
        _textDir = textDir;
        _autoGenPath = autoGenPath;
        _config = config;
    }

    public void Sync(bool forceReload)
    {
        bool changed = false;
        try
        {
            Directory.CreateDirectory(_textDir);
            AutoTranslatorBridge.EnsureAutoGenFile(_autoGenPath);
            foreach (var def in TranslationCategories.All)
            {
                string src = Path.Combine(_dictionariesDir, def.File);
                string dst = Path.Combine(_textDir, ManagedPrefix + def.File);
                if (_config.IsEnabled(def.Id) && File.Exists(src))
                {
                    string content = File.ReadAllText(src);
                    if (!File.Exists(dst) || File.ReadAllText(dst) != content)
                    {
                        File.WriteAllText(dst, content);
                        changed = true;
                    }
                }
                else if (File.Exists(dst))
                {
                    File.Delete(dst);
                    changed = true;
                }
            }

            foreach (var legacy in TranslationCategories.All.Select(c => c.File).Concat(new[] { "REPO_Cosmeticos.txt" }))
            {
                string p = Path.Combine(_textDir, legacy);
                if (File.Exists(p)) { File.Delete(p); changed = true; }
            }
        }
        catch (Exception ex)
        {
            BceConsole.LogWarning($"Falha ao sincronizar dicionários: {ex.Message}");
        }

        if (changed && forceReload)
        {
            if (AutoTranslatorBridge.TryReloadTranslations(out var err))
                BceConsole.LogInfo("Dicionários recarregados ao vivo.");
            else
                BceConsole.LogDebug($"Reload ao vivo indisponível ({err}); o file-watcher do AutoTranslator aplica em ~1s.");
        }
    }
}
