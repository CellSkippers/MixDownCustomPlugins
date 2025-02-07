using HarmonyLib;
using UnityEngine;

namespace MixedDownCustomPlugins.Patches
{
    [HarmonyPatch]
    internal class SurvivalWave_Patch
    {
        [HarmonyPatch(typeof(SurvivalWave), nameof(SurvivalWave.GetScoredSpawnPoint))]
        [HarmonyPrefix]
        public static bool SurvivalWave_GetScoredSpawnPoint_Patch(
            SurvivalWave __instance,
            ref SurvivalWave.ScoredSpawnPoint __result
        )
        {
            // Generate a vector which is actually uniform.
            float angle = UnityEngine.Random.Range(0.0f, 2 * Mathf.PI);
            Vector3 vector3 = new(Mathf.Cos(angle), 0.0f, Mathf.Sin(angle));
            __result = __instance.GetScoredSpawnPoint(vector3.normalized);
            return false;
        }
    }
}