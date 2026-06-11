using HarmonyLib;
using System;
using System.Text.RegularExpressions;

namespace RepoTraducaoPTBR;

internal static class QotlCompat
{
    internal static void Install(Harmony harmony)
    {
        var type = AccessTools.TypeByName("QueenOfTheLosers.Modifiers.TitleChange");
        if (type == null) return;

        var method = AccessTools.Method(type, "ReplaceTitle");
        if (method == null)
        {
            BceConsole.LogWarning("QueenOfTheLosers presente mas TitleChange.ReplaceTitle não foi encontrado — compat de título desativada.");
            return;
        }
        harmony.Patch(method, postfix: new HarmonyMethod(typeof(QotlCompat), nameof(ReplaceTitlePostfix)));
        BceConsole.LogDebug("Compat QueenOfTheLosers instalada (título da arena traduzido).");
    }

    private static void ReplaceTitlePostfix(string? title, ref string? __result)
    {
        if (__result == null || string.IsNullOrEmpty(title)) return;

        string pt = title!.ToUpperInvariant() switch
        {
            "QUEEN"   => "RAINHA",
            "KING"    => "REI",
            "MONARCH" => "MONARCA",
            "RULER"   => "SOBERANO",
            _         => title,
        };

        __result = Regex.Replace(__result, @"\bREI\b", pt.ToUpperInvariant());
        __result = Regex.Replace(__result, @"\bRei\b",
            char.ToUpperInvariant(pt[0]) + pt.Substring(1).ToLowerInvariant());
    }
}
