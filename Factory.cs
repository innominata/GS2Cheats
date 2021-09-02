using HarmonyLib;
using UnityEngine;

namespace GalacticScaleCheats
{
    public partial class GS2Cheats
    {
        //factoryOptions.Add(GSUI.Checkbox("Free Energy".Translate(), false, "freeEnergy", null, "Buildings do not require power".Translate()));
        //    factoryOptions.Add(GSUI.Checkbox("Fast Build".Translate(), false, "fastBuild", null, "Drones are lightning fast".Translate()));
        [HarmonyPrefix, HarmonyPatch(typeof(MechaDrone), "Update")]
        public static bool UpdateDrone(ref int __result, ref MechaDrone __instance, PrebuildData[] prebuildPool, Vector3 playerPos, float dt, ref double energy, ref double energyChange, double energyRate)
        {
            if (!active || !Preferences.GetBool("fastBuild")) return true;
            __instance.progress = 1f;
            energy += 1000000f;
            GameMain.mainPlayer.mecha.coreEnergy += energy;
            return true;
        }
    }
}