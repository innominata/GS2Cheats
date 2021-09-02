using HarmonyLib;

namespace GalacticScaleCheats
{
    public partial class GS2Cheats
    {
        //sphereOptions.Add(GSUI.Checkbox("Unlock Dyson Spheres".Translate(), false, "unlockDyson", null, "Start with the dyson sphere system unlocked".Translate()));
        //sphereOptions.Add(GSUI.Checkbox("Start with Full Sphere".Translate(), false, "fullSphere", null, "Remove tech requirement to access all latitudes".Translate()));
        [HarmonyPostfix]
        [HarmonyPatch(typeof(GameHistoryData), nameof(GameHistoryData.SetForNewGame))]
        public static void SetForNewGame_Postfix(GameHistoryData __instance)
        {
            if (active && Preferences.GetBool("fullSphere")) __instance.dysonNodeLatitude = 90f;
        }
        [HarmonyPrefix]
        [HarmonyPatch(typeof(GameHistoryData), nameof(GameHistoryData.dysonSphereSystemUnlocked), MethodType.Getter)]
        public static bool DysonSphereSystemUnlocked_Prefix(GameHistoryData __instance, ref bool __result)
        {
            if (!active || !Preferences.GetBool("unlockDyson")) return true;
            __result = true;
            return false;
        }
    }
}