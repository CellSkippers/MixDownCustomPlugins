using HarmonyLib;
using Player;

#nullable disable
namespace MixedDownCustomPlugins;

[HarmonyPatch]
internal static class Gladiator
{
    private const float STARTDELAY = 10f;
    private const float BOUTDURATION = 20f;
    private const float INTERMISSION = 5f;
    private const int NUMBEROFBOUTS = 8;
    private const float STOPBOUTSAFTER = STARTDELAY + NUMBEROFBOUTS * BOUTDURATION;

    private static float boutStartTimer = 0f;
    private static float boutEndTimer = 0f;
    private static bool inBout = false;
    private static bool finishedBouts = false;
    private static PlayerAgent localPlayerAgent = null;
    private static int playerSlotIndex = 0;
    private static bool isReset = true;

    private static void SetupState()
    {
        Logger.Info("Gladiator SetupState");
        boutStartTimer = 0f;
        boutEndTimer = 0f;
        inBout = false;
        finishedBouts = false;
        if (!PlayerManager.TryGetLocalPlayerAgent(out localPlayerAgent))
        {
            Logger.Error("No Local Player agent!");
        }
        playerSlotIndex = localPlayerAgent.PlayerSlotIndex;
        isReset = false;
    }

    private static void ResetState()
    {
        Logger.Info("Gladiator ResetState");
        boutStartTimer = 0f;
        boutEndTimer = 0f;
        inBout = false;
        finishedBouts = false;
        localPlayerAgent = null;
        playerSlotIndex = 0;
        isReset = true;
    }

    private static void GladiatorWarpIn()
    {
        switch (playerSlotIndex)
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

    private static void GladiatorWarpOut()
    {
        switch (playerSlotIndex)
        {
            case 0:
                localPlayerAgent.TryWarpTo(0, new UnityEngine.Vector3(0, 2, 30), new UnityEngine.Vector3(0, 0, 0));
                break;
            case 1:
                localPlayerAgent.TryWarpTo(0, new UnityEngine.Vector3(5, 2, 30), new UnityEngine.Vector3(0, 0, 0));
                break;
            case 2:
                localPlayerAgent.TryWarpTo(0, new UnityEngine.Vector3(-5, 2, 30), new UnityEngine.Vector3(0, 0, 0));
                break;
            case 3:
                localPlayerAgent.TryWarpTo(0, new UnityEngine.Vector3(0, 2, 20), new UnityEngine.Vector3(0, 0, 0));
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
        // Not in the correct level. Possibly reset the state, but mostly do nothing.
        if (GameStateManager.CurrentStateName != eGameStateName.InLevel)
        {
            if (!isReset)
            {
                ResetState();
            }
            return;
        }
        // Just dropped into correct level. Setup initial state.
        if (isReset)
        {
            SetupState();
            boutStartTimer = Clock.Time + STARTDELAY + playerSlotIndex * BOUTDURATION;
            boutEndTimer = boutStartTimer + BOUTDURATION - INTERMISSION;
        }

        // We're in the level!
        // Check if our bouts are finished.
        if (finishedBouts)
        {
            return;
        }
        // Check if we're ready for our bout to start.
        if (!inBout)
        {
            if (Clock.Time > boutStartTimer)
            {
                Logger.Info("Trying to warp in!");
                GladiatorWarpIn();
                inBout = true;
            }
        }
        // Check if we're ready for our bout to end.
        else
        {
            if (Clock.Time > boutEndTimer)
            {
                Logger.Info("Trying to warp out!");
                GladiatorWarpOut();
                inBout = false;

                // Calculate the timing of our next bout.
                boutStartTimer = Clock.Time + INTERMISSION + (PlayerManager.PlayerAgentsInLevel.Count - 1) * BOUTDURATION;
                boutEndTimer = boutStartTimer + BOUTDURATION - INTERMISSION;

                if (boutEndTimer > STOPBOUTSAFTER)
                {
                    finishedBouts = true;
                }
            }
        }
    }
}
