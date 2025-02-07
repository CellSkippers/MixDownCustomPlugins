using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;

namespace MixDownCustomPlugins;

[BepInPlugin(GUID, MODNAME, VERSION)]
[BepInDependency("CellSkippers.Parry", BepInDependency.DependencyFlags.HardDependency)]
public class Plugin : BasePlugin
{
    internal const string AUTHOR = "CellSkippers";
    internal const string MODNAME = "MixDownCustomPlugins";
    internal const string GUID = AUTHOR + "." + MODNAME;
    internal const string VERSION = "1.0.0";

    public override void Load()
    {
        Logger.SetupFromInit(this.Log);
        Logger.Info(MODNAME + " is loading...");
        ClassInjector.RegisterTypeInIl2Cpp<GladiatorWarpLoop>();
        new Harmony(GUID).PatchAll();
        Logger.Info(MODNAME + " loaded!");
    }
}