using System;

namespace RepoTraducaoPTBR;

internal static class BceConsole
{
    private const string InfoPrefix  = "[Info   :  REPO Tradução pt-BR] ";
    private const string WarnPrefix  = "[Warning:  REPO Tradução pt-BR] ";
    private const string ErrorPrefix = "[Error  :  REPO Tradução pt-BR] ";

    private static readonly Action<string, ConsoleColor>? _writeLine;

    static BceConsole()
    {
        var t = Type.GetType("BCE.console, BCE");
        if (t == null) return;

        try
        {
            var method = t.GetMethod("WriteLine", new[] { typeof(string), typeof(ConsoleColor) });
            if (method != null)
                _writeLine = (Action<string, ConsoleColor>)
                    Delegate.CreateDelegate(typeof(Action<string, ConsoleColor>), null, method);
        }
        catch (Exception ex)
        {
            Plugin.Log?.LogWarning(
                $"BceConsole: falha ao criar delegate ({ex.Message}). Saída BCE desativada — usando o logger do BepInEx.");
        }
    }

    internal static bool IsAvailable => _writeLine != null;

    internal static void LogInfo(string msg)
        => LogInfo(msg, ConsoleColor.Green);

    internal static void LogInfo(string msg, ConsoleColor color)
    {
        if (IsAvailable) _writeLine!(InfoPrefix + msg, color);
        else Plugin.Log?.LogInfo(msg);
    }

    internal static void LogWarning(string msg)
    {
        if (IsAvailable) _writeLine!(WarnPrefix + msg, ConsoleColor.Yellow);
        else Plugin.Log?.LogWarning(msg);
    }

    internal static void LogError(string msg)
    {
        if (IsAvailable) _writeLine!(ErrorPrefix + msg, ConsoleColor.Red);
        else Plugin.Log?.LogError(msg);
    }

    internal static void LogDebug(string msg)
        => Plugin.Log?.LogDebug(msg);
}
