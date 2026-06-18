using HarmonyLib;
using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace RepoTraducaoPTBR;

internal static class CurrencyPatch
{
    private static Func<bool> _enabled = null!;
    private static readonly CultureInfo PtBr = new CultureInfo("pt-BR");

    private const string GlitchTaxWord = "DIN";
    private const string GlitchMoney = "R$";

    private static volatile string? _lastGlitch;

    private static readonly Regex MoneyRx = new Regex(@"(?<![\w$=])\$ ?(?=\s*(?:<[^>]*>\s*)*\d)", RegexOptions.Compiled);

    private static readonly Regex ThousandRx = new Regex(@"(R\$ (?:\s|</?[^>]*>)*[\d.,]+(?:\s|</?[^>]*>)*)[Kk](?![A-Za-z])", RegexOptions.Compiled);

    private static readonly Regex ThousandSpriteRx = new Regex(@"(<sprite name=\$+>(?:\s|</?[^>]*>)*[\d.,]+(?:\s|</?[^>]*>)*)[Kk](?![A-Za-z])", RegexOptions.Compiled);

    private static readonly Regex DialogDollarRx = new Regex(@"(?<![\w=$])\$", RegexOptions.Compiled);

    internal static void Install(Func<bool> enabled, Harmony harmony)
    {
        _enabled = enabled;

        var setter = AccessTools.PropertySetter(AccessTools.TypeByName("TMPro.TMP_Text"), "text");
        if (setter == null)
            BceConsole.LogWarning("TMPro.TMP_Text.text não encontrado — troca de $ por R$ desativada (versão de TMP incompatível?).");
        else
            harmony.Patch(setter, prefix: new HarmonyMethod(typeof(CurrencyPatch), nameof(SymbolPrefix)));

        var dollar = AccessTools.Method("SemiFunc:DollarGetString", new[] { typeof(int) });
        if (dollar == null)
            BceConsole.LogWarning("SemiFunc.DollarGetString não encontrado — separador de milhar pt-BR desativado.");
        else
            harmony.Patch(dollar, postfix: new HarmonyMethod(typeof(CurrencyPatch), nameof(DollarPostfix)));

        var emoji = AccessTools.Method("SemiFunc:EmojiText", new[] { typeof(string) });
        if (emoji == null)
            BceConsole.LogWarning("SemiFunc.EmojiText não encontrado — 'R' antes do $ literal do diálogo desativado.");
        else
            harmony.Patch(emoji, postfix: new HarmonyMethod(typeof(CurrencyPatch), nameof(EmojiPostfix)));

        var glitch = new HarmonyMethod(typeof(CurrencyPatch), nameof(GlitchyPrefix));
        foreach (var type in new[] { "ValueScreen", "TextScreen", "ExtractionPoint" })
        {
            var m = AccessTools.Method($"{type}:GlitchyText");
            if (m != null)
                harmony.Patch(m, prefix: glitch);
        }
    }

    private static void SymbolPrefix(ref string value)
    {
        if (value == null || value.IndexOf('$') < 0) return;
        if (!_enabled()) return;
        if (ReferenceEquals(value, _lastGlitch)) return;
        value = MoneyRx.Replace(value, "R$$ ");
        if (value.IndexOf('K') >= 0 || value.IndexOf('k') >= 0)
        {
            value = ThousandRx.Replace(value, "$1 mil");
            value = ThousandSpriteRx.Replace(value, "$1 mil");
        }
    }

    private static bool GlitchyPrefix(ref string __result)
    {
        if (!_enabled()) return true;
        var sb = new System.Text.StringBuilder(12);
        int width = 0;
        while (width < 9)
        {
            if (width + GlitchTaxWord.Length <= 9 && UnityEngine.Random.Range(0, 4) == 0)
            {
                sb.Append(GlitchTaxWord);
                width += GlitchTaxWord.Length;
            }
            else if (width + GlitchMoney.Length <= 9 && UnityEngine.Random.Range(0, 3) == 0)
            {
                sb.Append(GlitchMoney);
                width += GlitchMoney.Length;
            }
            else
            {
                sb.Append(UnityEngine.Random.Range(0, 10));
                width += 1;
            }
        }
        __result = sb.ToString();
        _lastGlitch = __result;
        return false;
    }

    private static void DollarPostfix(int value, ref string __result)
    {
        if (!_enabled()) return;
        __result = value.ToString("#,0", PtBr);
    }

    private static void EmojiPostfix(ref string __result)
    {
        if (!_enabled()) return;
        if (__result == null || __result.IndexOf('$') < 0) return;
        __result = DialogDollarRx.Replace(__result, "R$$");
    }
}
