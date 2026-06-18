using System;
using System.IO;
using System.Linq;
using BepInEx;

namespace RepoTraducaoPTBR;

internal static class GhostFolderCleanup
{
    public static void Run()
    {
        try
        {
            string root = Path.Combine(Paths.BepInExRootPath, "Translation");
            if (!Directory.Exists(root)) return;

            string[] files = Directory.GetFiles(root, "*", SearchOption.AllDirectories);
            foreach (string f in files)
            {
                if (File.ReadAllText(f).Trim().Length > 0) return;
            }

            foreach (string f in files) File.Delete(f);
            foreach (string d in Directory.GetDirectories(root, "*", SearchOption.AllDirectories)
                                          .OrderByDescending(d => d.Length))
                Directory.Delete(d);
            Directory.Delete(root);
            BceConsole.LogDebug(@"Pasta residual BepInEx\Translation (vazia) removida.");
        }
        catch (Exception ex)
        {
            BceConsole.LogDebug($"Limpeza da pasta residual ignorada: {ex.Message}");
        }
    }
}
