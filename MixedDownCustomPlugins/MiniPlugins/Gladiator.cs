using Player;
using UnityEngine;

#nullable disable
namespace MixedDownCustomPlugins;

internal class Gladiator : MonoBehaviour
{
    private const float STARTDELAY = 30f;    // Time before bouts first start.
    private const float BOUTDURATION = 60f;  // Total time between bouts.
    private const float INTERMISSION = 5f;   // Intermission between bouts.
    private const int NUMBEROFBOUTS = 8;     // Total number of bouts.
        // Time a single player spends in a bout = BOUTDURATION - INTERMISSION

    private float boutStartTimer = 0f;
    private float boutEndTimer = 0f;
    private float stopBoutsTimer = 0f;
    private bool inBout = false;
    private bool finishedBouts = false;
    private PlayerAgent localPlayerAgent = null;
    private int playerSlotIndex = 0;

    public static void Enable()
    {
        Logger.DebugOnly("Gladiator Enable");
        if (PlayerManager.Current.gameObject.GetComponent<Gladiator>() == null)
            PlayerManager.Current.gameObject.AddComponent<Gladiator>();
    }

    public static void Disable()
    {
        Logger.DebugOnly("Gladiator Disable");
        if (PlayerManager.Current?.gameObject?.GetComponent<Gladiator>() != null)
            Destroy(PlayerManager.Current.gameObject.GetComponent<Gladiator>());
    }

    public void Start()
    {
        Logger.DebugOnly("Gladiator Start");
        float currentTime = Clock.Time;
        boutStartTimer = currentTime + STARTDELAY + playerSlotIndex * BOUTDURATION;
        boutEndTimer = boutStartTimer + BOUTDURATION - INTERMISSION;
        stopBoutsTimer = currentTime + STARTDELAY + (NUMBEROFBOUTS + 0.5f) * BOUTDURATION;
        inBout = false;
        finishedBouts = false;
        if (!PlayerManager.TryGetLocalPlayerAgent(out localPlayerAgent))
        {
            Logger.Error("No Local Player agent!");
        }
        playerSlotIndex = localPlayerAgent.PlayerSlotIndex;
    }

    public void Update()
    {
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
                GladiatorWarpIn();
                inBout = true;
            }
        }
        // Check if we're ready for our bout to end.
        else
        {
            if (Clock.Time > boutEndTimer)
            {
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

    private void GladiatorWarpIn()
    {
        Logger.DebugOnly("Trying to warp in!");
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

    private void GladiatorWarpOut()
    {
        Logger.DebugOnly("Trying to warp out!");
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
}
