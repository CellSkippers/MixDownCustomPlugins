using HarmonyLib;
using Player;

#nullable disable
namespace MixedDownCustomPlugins;

[HarmonyPatch]
internal static class Gladiator
{
    private const string ENABLEDLEVEL = "Gladiator";

    private const float STARTDELAY = 30f;
    private const float BOUTDURATION = 60f;
    private const float INTERMISSION = 5f;
    private const int NUMBEROFBOUTS = 8;

    private static float boutStartTimer = 0f;
    private static float boutEndTimer = 0f;
    private static float stopBoutsTimer = 0f;
    private static bool inBout = false;
    private static bool finishedBouts = false;
    private static PlayerAgent localPlayerAgent = null;
    private static int playerSlotIndex = 0;
    private static bool isReset = true;

    private static void SetupState()
    {
        Logger.DebugOnly("Gladiator SetupState");
        boutStartTimer = 0f;
        boutEndTimer = 0f;
        stopBoutsTimer = 0f;
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
        Logger.DebugOnly("Gladiator ResetState");
        boutStartTimer = 0f;
        boutEndTimer = 0f;
        stopBoutsTimer = 0f;
        inBout = false;
        finishedBouts = false;
        localPlayerAgent = null;
        playerSlotIndex = 0;
        isReset = true;
    }

    private static void GladiatorWarpIn()
    {
        bool successfullyWarped = false;
        switch (playerSlotIndex)
        {
            case 0:
                successfullyWarped = localPlayerAgent.TryWarpTo(0, new UnityEngine.Vector3(0, 1, 490), new UnityEngine.Vector3(0, 0, 1));
                break;
            case 1:
                successfullyWarped = localPlayerAgent.TryWarpTo(0, new UnityEngine.Vector3(0, 1, 530), new UnityEngine.Vector3(0, 0, -1));
                break;
            case 2:
                successfullyWarped = localPlayerAgent.TryWarpTo(0, new UnityEngine.Vector3(-20, 4.4f, 490), new UnityEngine.Vector3(1, 0, 1));
                break;
            case 3:
                successfullyWarped = localPlayerAgent.TryWarpTo(0, new UnityEngine.Vector3(20, 4.4f, 490), new UnityEngine.Vector3(-1, 0, 1));
                break;
            default:
                Logger.Warn("Invalid PlayerSlotIndex, not warping.");
                break;
        }
        if (!successfullyWarped)
        {
            Logger.Warn("Warp location for PlayerSlotIndex " + playerSlotIndex + " was invalid.");
        }
    }

    private static void GladiatorWarpOut()
    {
        int nextPlayerSlotIndex = playerSlotIndex + 1 == PlayerManager.PlayerAgentsInLevel.Count ? 0 : playerSlotIndex + 1;
        PlayerAgent nextPlayerAgent = PlayerManager.PlayerAgentsInLevel[nextPlayerSlotIndex];
        while (nextPlayerSlotIndex != playerSlotIndex)
        {
            // Try to warp out to the next player.
            if (localPlayerAgent.TryWarpTo(0, nextPlayerAgent.Position, nextPlayerAgent.Forward))
            {
                return;
            }
            // If it doesn't work, try the next player.
            nextPlayerSlotIndex = nextPlayerSlotIndex + 1 == PlayerManager.PlayerAgentsInLevel.Count ? 0 : nextPlayerSlotIndex + 1;
            nextPlayerAgent = PlayerManager.PlayerAgentsInLevel[nextPlayerSlotIndex];
        }
        Logger.Warn("No valid warp out found!");
    }

    [HarmonyPatch(typeof(PlayerManager), nameof(PlayerManager.Update))]
    [HarmonyPostfix]
    public static void GladiatorUpdate()
    {
        // Not in the correct level. Possibly reset the state, but mostly do nothing.
        if (GameStateManager.CurrentStateName != eGameStateName.InLevel || RundownManager.ActiveExpedition.Descriptive.PublicName != ENABLEDLEVEL)
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
            float currentTime = Clock.Time;
            boutStartTimer = currentTime + STARTDELAY + playerSlotIndex * BOUTDURATION;
            boutEndTimer = boutStartTimer + BOUTDURATION - INTERMISSION;
            stopBoutsTimer = currentTime + STARTDELAY + (NUMBEROFBOUTS + 0.5f) * BOUTDURATION;
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
                Logger.DebugOnly("Trying to warp in!");
                GladiatorWarpIn();
                inBout = true;
            }
        }
        // Check if we're ready for our bout to end.
        else
        {
            if (Clock.Time > boutEndTimer)
            {
                Logger.DebugOnly("Trying to warp out!");
                GladiatorWarpOut();
                inBout = false;

                // Calculate the timing of our next bout.
                boutStartTimer = Clock.Time + INTERMISSION + (PlayerManager.PlayerAgentsInLevel.Count - 1) * BOUTDURATION;
                boutEndTimer = boutStartTimer + BOUTDURATION - INTERMISSION;

                if (boutEndTimer > stopBoutsTimer)
                {
                    finishedBouts = true;
                }
            }
        }
    }
}
