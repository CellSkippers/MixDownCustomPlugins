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
    private static pMediumDamageData lastMediumDamageData;
    private static float shoveTime;
    private static Agent lastDamagingAgent;
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
            localPlayerAgent.GiveHealth(localPlayerAgent, ((UFloat16)(lastMediumDamageData.damage)).Get(9999f) * 0.01f);
            if (lastDamagingAgent != null)
            {
                DamageUtil.DoExplosionDamage(lastDamagingAgent.Position, 2f, 100f, LayerManager.MASK_EXPLOSION_TARGETS, LayerManager.MASK_EXPLOSION_BLOCKERS, true, 1500f);
            }
            //mSoundPlayer.Post("Parry", true);
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
        data.source.TryGet(out Agent damagingAgent);
        lastMediumDamageData = data;
        tookDamageTime = Clock.Time;
        lastDamagingAgent = damagingAgent;
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
        data.source.TryGet(out Agent damagingAgent);
        lastMediumDamageData = data;
        tookDamageTime = Clock.Time;
        lastDamagingAgent = damagingAgent;
        if (tookDamageTime > shoveTime && tookDamageTime - shoveTime < PARRYDURATION)
        {
            return false;
        }
        return true;
    }
}
