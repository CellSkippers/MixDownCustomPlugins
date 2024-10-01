using HarmonyLib;
using Player;

namespace MixedDownCustomPlugins;

[HarmonyPatch]
internal static class PlayerManagerPatch
{
    private static float timer = 0f;

    [HarmonyPatch(typeof(PlayerManager), nameof(PlayerManager.Update)
    )]
    [HarmonyPostfix]
    public static void GladiatorPatch()
	{
        if (PlayerManager.HasLocalPlayerAgent() && Clock.Time > timer)
        {
            Logger.Info("Trying to warp!");
            PlayerManager.GetLocalPlayerAgent().TryWarpTo(0, new UnityEngine.Vector3(0, 2, 20), new UnityEngine.Vector3(0, 0, 0));
            timer = Clock.Time + 10;
        }
	}
}
