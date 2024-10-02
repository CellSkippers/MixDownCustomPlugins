using HarmonyLib;
using Player;

#nullable disable
namespace MixedDownCustomPlugins;

[HarmonyPatch]
internal static class Gladiator
{
    private static float timer = 0f;
    private static PlayerAgent localPlayerAgent;
    private static bool isReset = true;

    private static void SetupState()
    {
        Logger.Info("Gladiator SetupState");
        if (PlayerManager.HasLocalPlayerAgent())
        {
            localPlayerAgent = PlayerManager.GetLocalPlayerAgent();
        }
        else
        {
            Logger.Error("No Local Player agent!");
        }
        isReset = false;
    }

    private static void ResetState()
    {
        Logger.Info("Gladiator ResetState");
        timer = 0f;
        localPlayerAgent = null;
        isReset = true;
    }

    private static void GladiatorWarp()
    {
        switch (localPlayerAgent.PlayerSlotIndex)
        {
            case 0:
                localPlayerAgent.TryWarpTo(0, new UnityEngine.Vector3(0, 2, 20), new UnityEngine.Vector3(0, 0, 0));
                break;
            case 1:
                localPlayerAgent.TryWarpTo(0, new UnityEngine.Vector3(0, 2, 30), new UnityEngine.Vector3(0, 0, 0));
                break;
            case 2:
                localPlayerAgent.TryWarpTo(0, new UnityEngine.Vector3(5, 2, 30), new UnityEngine.Vector3(0, 0, 0));
                break;
            case 3:
                localPlayerAgent.TryWarpTo(0, new UnityEngine.Vector3(-5, 2, 30), new UnityEngine.Vector3(0, 0, 0));
                break;
            default:
                Logger.Warn("Invalid PlayerSlotIndex, not warping.");
                break;
        }
    }

    [HarmonyPatch(typeof(PlayerManager), nameof(PlayerManager.Update))]
    [HarmonyPostfix]
    public static void GladiatorUpdate()
	{
        if (GameStateManager.CurrentStateName != eGameStateName.InLevel)
        {
            if (!isReset)
            {
                ResetState();
            }
            return;  // If we're not in a level and we're already reset, do nothing.
        }
        if (isReset)
        {
            SetupState();
        }
        if (Clock.Time > timer)
        {
            Logger.Info("Trying to warp!");
            GladiatorWarp();
            timer = Clock.Time + 10 + 5 * localPlayerAgent.PlayerSlotIndex;
        }
	}
}
