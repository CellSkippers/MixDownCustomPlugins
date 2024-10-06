using Agents;
using Gear;
using HarmonyLib;
using Player;
using SNetwork;
using UnityEngine;

#nullable disable
namespace MixedDownCustomPlugins;

[HarmonyPatch]
internal static class Parry
{
    private const string ENABLEDLEVEL = "Gladiator";

    private const float PARRYDURATION = 0.3f;

    private static float tookDamageTime;
    private static pMediumDamageData lastDamageDataMedium;
    private static float shoveTime;
    private static Agent lastAgentDamage;
    private static bool parrySuccess;

    [HarmonyPatch(typeof(PlayerManager), nameof(PlayerManager.Update))]
    [HarmonyPostfix]
    public static void ParryUpdate()
    {
        // Not in the correct level, do nothing.
        if (GameStateManager.CurrentStateName != eGameStateName.InLevel || RundownManager.ActiveExpedition.Descriptive.PublicName != ENABLEDLEVEL)
        {
            return;
        }

        if (tookDamageTime > shoveTime && tookDamageTime - shoveTime < PARRYDURATION)
        {
            parrySuccess = true;
            PlayerAgent localPlayerAgent = PlayerManager.GetLocalPlayerAgent();
            localPlayerAgent.GiveHealth(localPlayerAgent, ((UFloat16)(/*ref*/ lastDamageDataMedium.damage)).Get(9999f) * 0.01f);
            if ((UnityEngine.Object)(object)lastAgentDamage != (UnityEngine.Object)null)
            {
                DamageUtil.DoExplosionDamage(lastAgentDamage.Position, 2f, 100f, LayerManager.MASK_EXPLOSION_TARGETS, LayerManager.MASK_EXPLOSION_BLOCKERS, true, 1500f);
            }
            //mSoundPlayer.Post("Parry", true);
            //Debug.Log(Object.op_Implicit((Object)(object)LastAgentDamage == (Object)null));
            shoveTime = -11f;
            parrySuccess = false;
        }
    }

    [HarmonyPatch(typeof(MWS_Push), nameof(MWS_Push.Enter))]
    [HarmonyPostfix]
    public static void ParryEnter()
    {
        shoveTime = Clock.Time;
    }

    [HarmonyPatch(typeof(Dam_PlayerDamageLocal), nameof(Dam_PlayerDamageLocal.ReceiveShooterProjectileDamage))]
    [HarmonyPrefix]
    public static bool ParryShooterProjectileDamage(pMediumDamageData data)
    {
        Agent lastAgentDamage = default(Agent);
        ((pAgent)(/*ref*/ data.source)).TryGet(/*ref*/ out lastAgentDamage);
        //Debug.Log(Object.op_Implicit("Took shooter damage"));
        lastDamageDataMedium = data;
        tookDamageTime = Clock.Time;
        lastAgentDamage = lastAgentDamage;
        if (tookDamageTime > shoveTime && tookDamageTime - shoveTime < PARRYDURATION)
        {
            return false;
        }
        return true;
    }

    [HarmonyPatch(typeof(Dam_PlayerDamageLocal), nameof(Dam_PlayerDamageLocal.ReceiveTentacleAttackDamage))]
    [HarmonyPrefix]
    public static bool ParryTentacleAttackDamage(pMediumDamageData data)
    {
        Agent lastAgentDamage;
        ((pAgent)(/*ref*/ data.source)).TryGet(out lastAgentDamage);
        lastDamageDataMedium = data;
        tookDamageTime = Clock.Time;
        lastAgentDamage = lastAgentDamage;
        if (tookDamageTime - shoveTime > 0f && tookDamageTime - shoveTime < PARRYDURATION)
        {
            return false;
        }
        return true;
    }
}
