using System.Collections.Generic;

namespace RepoTraducaoPTBR;

internal enum TranslationCategory
{
    Game,
    ModConfigs,
    MoreHeadBridge,
    VanillaCosmetics,
    XuaunCosmetics,
    ModdedCosmetics,
}

internal sealed class CategoryDef
{
    public TranslationCategory Id { get; }
    public string File { get; }
    public string Key { get; }
    public string Description { get; }

    public CategoryDef(TranslationCategory id, string file, string key, string description)
    {
        Id = id;
        File = file;
        Key = key;
        Description = description;
    }
}

internal static class TranslationCategories
{
    public static readonly IReadOnlyList<CategoryDef> All = new[]
    {
        new CategoryDef(TranslationCategory.Game, "Jogo.txt", "Translate Game",
            "R.E.P.O.'s own menus, HUD, settings and controls (native localization + dictionary)."),
        new CategoryDef(TranslationCategory.ModConfigs, "Config.txt", "Translate Mod Configs",
            "Mod configuration menus (REPOConfig, CustomGrabColor, Mimic…)."),
        new CategoryDef(TranslationCategory.MoreHeadBridge, "MoreHeadBridge.txt", "Translate MoreHead Bridge",
            "MoreHead Bridge interface (tabs, Customizer, Mini-Semibot…)."),
        new CategoryDef(TranslationCategory.VanillaCosmetics, "CosmeticosVanilla.txt", "Translate Vanilla Cosmetics",
            "Vanilla cosmetic names (they come from assetName; there is no official translation for them)."),
        new CategoryDef(TranslationCategory.XuaunCosmetics, "CosmeticosXuaun.txt", "Translate Xuaun Cosmetics",
            "Cosmetic names from Xuaun mods (XuaunCosmetics, RepoPride, FortniteSemibot, YoshyCarry, MonsterCosmetics)."),
        new CategoryDef(TranslationCategory.ModdedCosmetics, "CosmeticosModded.txt", "Translate Modded Cosmetics (Others)",
            "Cosmetic names from other mods (e.g.: MoreHead, BASICS, etc.)."),
    };
}
