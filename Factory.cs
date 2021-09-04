using GalacticScale;
using HarmonyLib;
using UnityEngine;

namespace GalacticScaleCheats
{
    public partial class GS2Cheats
    {
        //factoryOptions.Add(GSUI.Checkbox("Free Energy".Translate(), false, "freeEnergy", null, "Buildings do not require power".Translate()));
        //    factoryOptions.Add(GSUI.Checkbox("Fast Build".Translate(), false, "fastBuild", null, "Drones are lightning fast".Translate()));
        [HarmonyPostfix]
        [HarmonyPatch(typeof(MechaDrone), "Update")]
        public static void UpdateDrone(ref int __result, ref MechaDrone __instance, PrebuildData[] prebuildPool, Vector3 playerPos, float dt, ref double energy, ref double energyChange, double energyRate)
        {
            if (!active || !Preferences.GetBool("fastBuild")) return;
            GS2.Warn($"FastBuild {dt} {energy} {energyChange} {energyRate}");
            if (__instance.progress > 0) __instance.progress = 1f;
            energy += energyRate;
            GameMain.mainPlayer.mecha.coreEnergy -= energyChange;
            __instance.position = __instance.target;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Mecha), "get_droneCount")]
        public static bool get_droneCount(ref int __result)
        {
            if (!active || !Preferences.GetBool("fastBuild")) return true;
            __result = 256;
            return false;
        }
    }
}