using GalacticScale;
using HarmonyLib;

namespace GalacticScaleCheats
{
    public partial class GS2Cheats
    {
        //    playerOptions.Add(GSUI.Checkbox("Unlimited Energy".Translate(), false, "unlimitedEnergy", null, "Never run out of core power".Translate()));
        //    playerOptions.Add(GSUI.Checkbox("Unlock Sail".Translate(), false, "unlockSail", null, "Start game able to fly".Translate()));
        //    playerOptions.Add(GSUI.Checkbox("Unlock Warp".Translate(), false, "unlockWarp", null, "Start game able to warp".Translate()));
        //    playerOptions.Add(GSUI.Checkbox("Always Warp".Translate(), false, "alwaysWarp", null, "Never require warpers to warp".Translate()));
        //    playerOptions.Add(GSUI.Checkbox("Boost Walk Speed".Translate(), false, "boostWalkSpeed", null, "Walk faster".Translate()));
        //    playerOptions.Add(GSUI.Checkbox("Boost Sail Speed".Translate(), false, "boostSailSpeed", null, "Sail faster".Translate()));
        //    playerOptions.Add(GSUI.Checkbox("Boost Warp Speed".Translate(), false, "boostWarpSpeed", null, "Warp faster".Translate()));
        private void MechaUpdate()
        {
            if (VFInput.alt && VFInput._moveRight) GS2.Warn(active.ToString());
            if (!active) return;
            var mecha = GameMain.mainPlayer.mecha;
            if (Preferences.GetBool("unlimitedEnergy")) mecha.coreEnergy = mecha.coreEnergyCap;
            // if (preferences.GetBool("unlockSail", false))
            // {
            //     //GameMain
            // }
        }

        private static void MechaApply()
        {
            if (active && Preferences.GetBool("unlockSail")) GameMain.mainPlayer.mecha.thrusterLevel = 4;
            if (active && Preferences.GetBool("unlockWarp")) GameMain.mainPlayer.mecha.thrusterLevel = 5;
            if (active && Preferences.GetBool("boostWalkSpeed")) GameMain.mainPlayer.mecha.walkSpeed = 25;
            if (active && Preferences.GetBool("boostSailSpeed")) GameMain.mainPlayer.mecha.maxSailSpeed = 2000f;
            if (active && Preferences.GetBool("boostWarpSpeed")) GameMain.mainPlayer.mecha.maxWarpSpeed = 1000000f;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(GameHistoryData), nameof(GameHistoryData.SetForNewGame))]
        public static void SetForNewGame_PlayerPostfix(GameHistoryData __instance)
        {
            if (active && Preferences.GetBool("unlockSail")) GameMain.mainPlayer.mecha.thrusterLevel = 4;
            if (active && Preferences.GetBool("unlockWarp")) GameMain.mainPlayer.mecha.thrusterLevel = 5;
            if (active && Preferences.GetBool("boostWalkSpeed")) GameMain.mainPlayer.mecha.walkSpeed = 25;
            if (active && Preferences.GetBool("boostSailSpeed")) GameMain.mainPlayer.mecha.walkSpeed = 4000f;
            if (active && Preferences.GetBool("boostWarpSpeed")) GameMain.mainPlayer.mecha.walkSpeed = 1000000f;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Mecha), "UseWarper")]
        public static void UseWarper_Postfix(ref bool __result)
        {
            if (active && Preferences.GetBool("alwaysWarp")) __result = true;
        }
    }
}