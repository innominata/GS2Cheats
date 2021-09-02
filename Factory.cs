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


        [HarmonyPrefix]
        [HarmonyPatch(typeof(PowerSystem), "GameTick")]
        public static void PowerTickPrefix(ref PowerSystem __instance, ref PowerConsumerComponent[] __state)
        {
            if (!active || !Preferences.GetBool("freeEnergy")) return;
            __state = new PowerConsumerComponent[__instance.consumerPool.Length];
            __instance.consumerPool.CopyTo(__state, 0);
            for (int i = 0; i < __instance.consumerPool.Length; i++)
            {
                __instance.consumerPool[i].idleEnergyPerTick = 0;
                __instance.consumerPool[i].workEnergyPerTick = 0;
                __instance.consumerPool[i].requiredEnergy = 0;
            }
        }
        [HarmonyPostfix]
        [HarmonyPatch(typeof(PowerSystem), "GameTick")]
        public static void PowerTickPostfix(ref PowerSystem __instance, ref PowerConsumerComponent[] __state)
        {
            if (!active || !Preferences.GetBool("freeEnergy")) return;
            __instance.consumerPool = __state;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(StationComponent), "CalcTripEnergyCost")]
        public static bool StationComponentEnergyNeededPrefix(ref StationComponent __instance, ref long __result)
        {
            if (!active || !Preferences.GetBool("freeEnergy")) return true;
            __instance.energy = __instance.energyMax;
            __result = 0L;
            return false;
        }
    }
}