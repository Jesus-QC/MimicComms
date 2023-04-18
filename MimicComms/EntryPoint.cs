using HarmonyLib;
using PluginAPI.Core;
using PluginAPI.Core.Attributes;

namespace MimicComms;

public class EntryPoint
{
    private const string Version = "1.0.0.0";

    private static readonly Harmony Harmony = new("com.jesusqc.mimiccomms");
    
    [PluginEntryPoint("MimicComms", Version, "Makes 939 able to talk in the intercom.", "Jesus-QC")]
    public void Main()
    {
        Log.Raw($"<color=green>MimicComms {Version} by Jesus-QC is being loaded...</color>");
        Harmony.PatchAll();
    }
}
