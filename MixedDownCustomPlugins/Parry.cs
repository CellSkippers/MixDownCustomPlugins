using Agents;
using Gear;
using HarmonyLib;
using Player;

#nullable disable
namespace MixedDownCustomPlugins;

[HarmonyPatch]
internal static class Parry
{
    private const string ENABLEDLEVEL = "Gladiator";

    private const float PARRYDURATION = 0.3f;

    private static float shoveTime;

    // Return value is used by the relevant receive damage prefix to determine whether the original receive damage method should run.
    private static bool SuccessfullyParry(pMediumDamageData damageData)
    {
        if (RundownManager.ActiveExpedition.Descriptive.PublicName != ENABLEDLEVEL)
        {
            // Wrong level, do nothing and run the original receive damage method.
            return true;
        }

        // Heal the player.
        PlayerAgent localPlayerAgent = PlayerManager.GetLocalPlayerAgent();
        localPlayerAgent.GiveHealth(localPlayerAgent, damageData.damage.Get(9999f) * 0.01f);

        // Explode the enemy.
        damageData.source.TryGet(out Agent damagingAgent);
        if (damagingAgent != null)
        {
            DamageUtil.DoExplosionDamage(damagingAgent.Position, 2f, 100f, LayerManager.MASK_EXPLOSION_TARGETS, LayerManager.MASK_EXPLOSION_BLOCKERS, true, 1500f);
        }

        // Don't run the original receive damage method - player doesn't take the damage.
        return false;
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
        float tookDamageTime = Clock.Time;
        if (tookDamageTime > shoveTime && tookDamageTime - shoveTime < PARRYDURATION)
        {
            return SuccessfullyParry(data);
        }
        return true;
    }

    [HarmonyPatch(typeof(Dam_PlayerDamageLocal), nameof(Dam_PlayerDamageLocal.ReceiveTentacleAttackDamage))]
    [HarmonyPrefix]
    public static bool ParryTentacleAttackDamage(pMediumDamageData data)
    {
        float tookDamageTime = Clock.Time;
        if (tookDamageTime > shoveTime && tookDamageTime - shoveTime < PARRYDURATION)
        {
            return SuccessfullyParry(data);
        }
        return true;
    }
}
