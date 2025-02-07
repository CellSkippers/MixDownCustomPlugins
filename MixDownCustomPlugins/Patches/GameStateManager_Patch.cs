using HarmonyLib;

namespace MixDownCustomPlugins.Patches
{
    [HarmonyPatch]
    internal class GameStateManager_Patch
    {
        [HarmonyPatch(typeof(GameStateManager), nameof(GameStateManager.ChangeState))]
        [HarmonyPostfix]
        public static void GamestateManager_ChangeState_Patch(eGameStateName nextState)
        {
            bool startingLevel = nextState == eGameStateName.InLevel;
            pActiveExpedition activeExpedition = RundownManager.GetActiveExpeditionData();

            // A1
            if (startingLevel && activeExpedition.tier == eRundownTier.TierA && activeExpedition.expeditionIndex == 0)
            {
                
            }
            // A2
            if (startingLevel && activeExpedition.tier == eRundownTier.TierA && activeExpedition.expeditionIndex == 1)
            {

            }
            // A3
            if (startingLevel && activeExpedition.tier == eRundownTier.TierA && activeExpedition.expeditionIndex == 2)
            {

            }
            // A4
            if (startingLevel && activeExpedition.tier == eRundownTier.TierA && activeExpedition.expeditionIndex == 3)
            {

            }
            // B1
            if (startingLevel && activeExpedition.tier == eRundownTier.TierB && activeExpedition.expeditionIndex == 0)
            {

            }
            // B2
            if (startingLevel && activeExpedition.tier == eRundownTier.TierB && activeExpedition.expeditionIndex == 1)
            {
                GladiatorWarpLoop.Enable();
                Parry.Patch.parryEnabled = true;
            }
            else
            {
                GladiatorWarpLoop.Disable();
                Parry.Patch.parryEnabled = false;
            }
        }
    }
}
